using UnityEngine;

namespace Playbox.Purchases
{
    public class GooglePlayPurchaseManager : BasePurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("GooglePlayPurchaseManager Initialized");
        }

        public override void Purchase(string productId)
        {
            var product = _products.Find(p => p.Id == productId);
            if (product != null)
            {
                Debug.Log($"GooglePlayPurchaseManager: Purchase {productId} succeeded");
                TriggerPurchaseSuccess(product);
            }
            else
                TriggerPurchaseFailed(null, $"Product {productId} not found");
        }
    }
}