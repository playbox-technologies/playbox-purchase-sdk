using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Playbox.Purchases
{
    /// <summary>
    /// Manages the in-app purchase process using Unity IAP, handling product fetch, purchase, and restoration.
    /// </summary>
    public class UniversalPurchaseManager : PurchaseManager
    {
        private StoreController _storeController;

        /// <summary>
        /// Initializes the purchase manager and connects to Unity IAP services.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("[IAP] PurchaseManager.Initialize()");
            InitializeIapAsync();
        }

        /// <summary>
        /// Asynchronously initializes Unity IAP services and calls the method to initialize the store controller.
        /// </summary>
        private async void InitializeIapAsync()
        {
            try
            {
                Debug.Log("[IAP] UnityServices.InitializeAsync()...");
                await UnityServices.InitializeAsync();
                Debug.Log("[IAP] UGS initialized OK");

                await InitializeIapV5Async();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[IAP] Failed to initialize UGS/IAP: {e}");
            }
        }

        /// <summary>
        /// Initializes the store controller and sets up event handlers for various purchase events.
        /// </summary>
        private async Task InitializeIapV5Async()
        {
            Debug.Log("[IAP] UnityIAPServices.StoreController()...");
            _storeController = UnityIAPServices.StoreController();

            // Setting up event handlers for product fetch, purchases, and store connection events
            _storeController.OnProductsFetched += OnProductsFetched;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            _storeController.OnPurchasesFetched += OnPurchasesFetched;
            _storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            _storeController.OnPurchasePending += OnPurchasePending;
            _storeController.OnPurchaseFailed += OnPurchasesFailed;
            _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            _storeController.OnStoreDisconnected += OnStoreDisconnected;
            _storeController.OnPurchaseDeferred += OnPurchaseDeferred;

            Debug.Log("[IAP] StoreController.Connect()...");
            await _storeController.Connect();
            Debug.Log("[IAP] Store connected OK");

            var defs = new List<ProductDefinition>();
            foreach (var p in _products)
            {
                var unityType = p.Type == ProductType.NonConsumable
                    ? UnityEngine.Purchasing.ProductType.NonConsumable
                    : UnityEngine.Purchasing.ProductType.Consumable;

                Debug.Log($"[IAP] Add ProductDefinition: {p.Id}, type={p.Type}");
                defs.Add(new ProductDefinition(p.Id, unityType));
            }

            Debug.Log("[IAP] FetchProducts()...");
            _storeController.FetchProducts(defs);
        }

        /// <summary>
        /// Handles deferred purchases when the purchase is not immediately confirmed.
        /// </summary>
        private void OnPurchaseDeferred(DeferredOrder order)
        {
            var id = order.CartOrdered.Items().FirstOrDefault()?.Product?.definition.id;
            Debug.Log($"[IAP] OnPurchaseDeferred: {id} - purchase is deferred");
        }

        /// <summary>
        /// Handles store disconnection events and logs a warning message.
        /// </summary>
        private void OnStoreDisconnected(StoreConnectionFailureDescription desc)
        {
            Debug.LogWarning($"[IAP] Store disconnected: reason={desc}, store={desc}");
        }

        /// <summary>
        /// Handles the event when products are fetched successfully and triggers the fetch of purchases.
        /// </summary>
        private void OnProductsFetched(List<Product> products)
        {
            Debug.Log($"[IAP] OnProductsFetched: count={products?.Count}");

            _storeController.FetchPurchases();
        }

        /// <summary>
        /// Handles the failure of fetching products and logs an error message.
        /// </summary>
        private void OnProductsFetchFailed(ProductFetchFailed error)
        {
            Debug.LogError($"[IAP] OnProductsFetchFailed: {error}");
        }

        /// <summary>
        /// Handles the event when purchases are fetched and processes the restored purchases.
        /// </summary>
        private void OnPurchasesFetched(Orders orders)
        {
            Debug.Log("[IAP] OnPurchasesFetched — processing restore");

            foreach (var order in orders.ConfirmedOrders)
            {
                var id = order.CartOrdered.Items().FirstOrDefault()?.Product.definition.id;
                Debug.Log($"Restore → {id}");

                var product = _products.FirstOrDefault(x => x.Id == id);
                if (product == null) continue;

                if (product.Type == ProductType.NonConsumable)
                    SaveNonConsumable(id);
            }
        }

        /// <summary>
        /// Handles the failure of fetching purchases and logs an error message.
        /// </summary>
        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription error)
        {
            Debug.LogError($"[IAP] OnPurchasesFetchFailed: {error}");
        }

        /// <summary>
        /// Handles pending orders and confirms the purchase when the product is recognized.
        /// </summary>
        private void OnPurchasePending(PendingOrder order)
        {
            var item = order.CartOrdered.Items().FirstOrDefault();
            var id = item?.Product?.definition.id;

            Debug.Log($"[IAP] OnPurchasePending: {id}");

            if (id == null)
            {
                _storeController.ConfirmPurchase(order);
                return;
            }

            var product = _products.FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                Debug.LogWarning($"Unknown product: {id}");
                _storeController.ConfirmPurchase(order);
                return;
            }

#if PLAYBOX_SDK
            var unityProduct = item.Product;

            var adapter = new ProductDataAdapter
            {
                TransactionId = unityProduct.transactionID,
                DefinitionId = unityProduct.definition.id,
                MetadataLocalizedPrice = unityProduct.metadata.localizedPrice,
                MetadataIsoCurrencyCode = unityProduct.metadata.isoCurrencyCode,
                Receipt = unityProduct.receipt
            };

            Analytics.LogPurchase(adapter, isValid => { });
#endif

            if (product.Type == ProductType.NonConsumable)
                SaveNonConsumable(id);

            TriggerPurchaseSuccess(product);

            _storeController.ConfirmPurchase(order);
        }

        /// <summary>
        /// Handles the event when a purchase is confirmed.
        /// </summary>
        private void OnPurchaseConfirmed(Order order)
        {
            var id = order.CartOrdered.Items().FirstOrDefault()?.Product.definition.id;
            Debug.Log($"[IAP] OnPurchaseConfirmed: {id}");
        }

        /// <summary>
        /// Restores previously made purchases on iOS.
        /// </summary>
        public void RestorePurchases()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _storeController.RestoreTransactions((success, error) =>
                {
                    if (success)
                    {
                        Debug.Log("[IAP] Successfully restored transactions.");
                    }
                    else
                    {
                        Debug.LogWarning("[IAP] Failed to restore transactions. Error: " + error);
                    }
                });
            }
        }

        /// <summary>
        /// Handles failed purchases and triggers the failure event.
        /// </summary>
        private void OnPurchasesFailed(FailedOrder order)
        {
            var id = order.CartOrdered.Items().FirstOrDefault()?.Product?.definition.id;

            Debug.LogWarning($"[IAP] Purchase FAILED: {id}, reason={order.FailureReason}");

            var product = _products.FirstOrDefault(x => x.Id == id);
            TriggerPurchaseFailed(product, order.FailureReason.ToString());
        }

        /// <summary>
        /// Initiates the purchase process for a given product.
        /// </summary>
        protected override void ProcessPlatformPurchase(IProduct product)
        {
            Debug.Log($"[IAP] Request purchase → {product?.Id}");

            if (_storeController == null)
            {
                TriggerPurchaseFailed(product, "IAP not initialized");
                return;
            }

#if PLAYBOX_SDK
            var adapter = new ProductDataAdapter
            {
                DefinitionId = product.Id
            };

            Analytics.LogPurshaseInitiation(adapter);
#endif

            _storeController.PurchaseProduct(product.Id);
        }
    }
}