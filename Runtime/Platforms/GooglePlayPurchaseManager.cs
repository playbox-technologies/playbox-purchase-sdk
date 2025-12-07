using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;

namespace Playbox.Purchases
{
    public class GooglePlayPurchaseManager : PurchaseManager, IStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("[IAP] GooglePlayPurchaseManager.Initialize()");
            InitializeIapAsync();
        }

        private async void InitializeIapAsync()
        {
            try
            {
                Debug.Log("[IAP] UnityServices.InitializeAsync()...");
                await UnityServices.InitializeAsync();
                Debug.Log("[IAP] UGS initialized OK");

                var module = StandardPurchasingModule.Instance();
                var builder = ConfigurationBuilder.Instance(module);

                foreach (var p in _products)
                {
                    var unityType = p.Type == ProductType.NonConsumable
                        ? UnityEngine.Purchasing.ProductType.NonConsumable
                        : UnityEngine.Purchasing.ProductType.Consumable;

                    Debug.Log($"[IAP] AddProduct: {p.Id}, type={p.Type}");
                    builder.AddProduct(p.Id, unityType);
                }

                Debug.Log("[IAP] UnityPurchasing.Initialize()...");
                UnityPurchasing.Initialize(this, builder);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[IAP] Failed to initialize UGS/IAP: {e}");
            }
        }

        protected override void ProcessPlatformPurchase(IProduct product)
        {
            Debug.Log($"[IAP] ProcessPlatformPurchase: {product?.Id}");

            if (_controller == null)
            {
                Debug.LogWarning("[IAP] IAP not initialized (_controller == null)");
                TriggerPurchaseFailed(product, "IAP not initialized");
                return;
            }

            Debug.Log($"[IAP] InitiatePurchase: {product.Id}");
            _controller.InitiatePurchase(product.Id);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            Debug.Log("[IAP] OnInitialized: IAP initialized OK");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[IAP] OnInitializeFailed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[IAP] OnInitializeFailed: {error} - {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var unityProduct = args.purchasedProduct;
            Debug.Log($"[IAP] ProcessPurchase: {unityProduct.definition.id}");

            var product = _products.FirstOrDefault(p => p.Id == unityProduct.definition.id);
            if (product == null)
            {
                Debug.LogWarning($"[IAP] Unknown product: {unityProduct.definition.id}");
                return PurchaseProcessingResult.Complete;
            }

            Debug.Log($"[IAP] Recognized product: {product.Id}, type={product.Type}");

            if (product.Type == ProductType.NonConsumable)
            {
                Debug.Log($"[IAP] SaveNonConsumable({product.Id})");
                SaveNonConsumable(product.Id);
            }
            else if (product.Type == ProductType.Consumable)
            {
                Debug.Log($"[IAP] AddConsumable({product.Id}, 1)");
                AddConsumable(product.Id, 1);
            }

            Debug.Log($"[IAP] Purchase SUCCESS: {product.Id}");
            TriggerPurchaseSuccess(product);

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(UnityEngine.Purchasing.Product unityProduct, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"[IAP] OnPurchaseFailed: {unityProduct.definition.id}, reason={failureReason}");

            var product = _products.FirstOrDefault(p => p.Id == unityProduct.definition.id);
            TriggerPurchaseFailed(product, failureReason.ToString());
        }
    }
}