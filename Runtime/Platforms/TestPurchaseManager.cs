using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;
using Unity.Services.Core;

namespace Playbox.Purchases
{
    public class TestPurchaseManager : PurchaseManager, IStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extensions;

        public override void Initialize()
        {
            base.Initialize();

#if UNITY_EDITOR
            var module = StandardPurchasingModule.Instance();
            module.useFakeStoreAlways = true;
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif

            Debug.Log("[TestPurchaseManager] Initialized (fake IAP)");
            InitializeIapAsync();
        }

        private async void InitializeIapAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("[TestPurchaseManager] UGS initialized");

                var module = StandardPurchasingModule.Instance();
                var builder = ConfigurationBuilder.Instance(module);

                foreach (var p in _products)
                {
                    var unityType = p.Type == ProductType.NonConsumable
                        ? UnityEngine.Purchasing.ProductType.NonConsumable
                        : UnityEngine.Purchasing.ProductType.Consumable;

                    builder.AddProduct(p.Id, unityType);
                }

                UnityPurchasing.Initialize(this, builder);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TestPurchaseManager] Failed to initialize UGS/IAP: {e}");
            }
        }

        protected override void ProcessPlatformPurchase(IProduct product)
        {
            if (_controller == null)
            {
                Debug.LogWarning("[TestPurchaseManager] IAP not initialized (_controller == null)");
                TriggerPurchaseFailed(product, "IAP not initialized");
                return;
            }

            _controller.InitiatePurchase(product.Id);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            Debug.Log("[TestPurchaseManager] IAP initialized");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[TestPurchaseManager] OnInitializeFailed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[TestPurchaseManager] OnInitializeFailed: {error} - {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var unityProduct = args.purchasedProduct;
            var product = _products.FirstOrDefault(p => p.Id == unityProduct.definition.id);

            if (product == null)
            {
                Debug.LogWarning($"[TestPurchaseManager] Unknown product: {unityProduct.definition.id}");
                return PurchaseProcessingResult.Complete;
            }

            if (product.Type == ProductType.NonConsumable)
            {
                SaveNonConsumable(product.Id);
            }
            else if (product.Type == ProductType.Consumable)
            {
                AddConsumable(product.Id, 1);
            }

            TriggerPurchaseSuccess(product);
            return PurchaseProcessingResult.Complete;
        }

        new public void OnPurchaseFailed(UnityEngine.Purchasing.Product unityProduct, PurchaseFailureReason failureReason)
        {
            var product = _products.FirstOrDefault(p => p.Id == unityProduct.definition.id);
            TriggerPurchaseFailed(product, failureReason.ToString());
        }
    }
}