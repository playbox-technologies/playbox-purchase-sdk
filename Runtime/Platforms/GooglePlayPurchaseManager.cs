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
            InitializAsync();
            Debug.Log("GooglePlayPurchaseManager: Initialize started");

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

        private async void InitializAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("UGS initialized");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize UGS: {e}");
            }
        }

        protected override void ProcessPlatformPurchase(IProduct product)
        {
            if (_controller == null)
            {
                Debug.LogWarning("GooglePlayPurchaseManager: IAP not initialized");
                TriggerPurchaseFailed(product, "IAP not initialized");
                return;
            }

            Debug.Log($"GooglePlayPurchaseManager: Initiate purchase {product.Id}");
            _controller.InitiatePurchase(product.Id);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _controller = controller;
            _extensions = extensions;
            Debug.Log("GooglePlayPurchaseManager: IAP initialized");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"GooglePlayPurchaseManager: IAP init failed: {error}");
        }

#if UNITY_2019_1_OR_NEWER
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"GooglePlayPurchaseManager: IAP init failed: {error} - {message}");
        }
#endif

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var unityProduct = args.purchasedProduct;

            var product = _products.FirstOrDefault(p => p.Id == unityProduct.definition.id);
            if (product == null)
            {
                Debug.LogWarning($"GooglePlayPurchaseManager: Unknown product {unityProduct.definition.id}");
                return PurchaseProcessingResult.Complete;
            }

            if (product.Type == ProductType.NonConsumable)
                SaveNonConsumable(product.Id);
            else if (product.Type == ProductType.Consumable)
                AddConsumable(product.Id, 1);
            TriggerPurchaseSuccess(product);

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(UnityEngine.Purchasing.Product unityProduct, PurchaseFailureReason failureReason)
        {
            var product = _products.FirstOrDefault(p => p.Id == unityProduct.definition.id);
            TriggerPurchaseFailed(product, failureReason.ToString());
        }
    }
}