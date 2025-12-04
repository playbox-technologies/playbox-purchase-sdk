using UnityEngine;

namespace Playbox.Purchases
{
    public class AppStorePurchaseManager : BasePurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("AppStorePurchaseManager Initialized");
        }

        public override void Purchase(string productId)
        {
            var product = _products.Find(p => p.Id == productId);
            if (product != null)
            {
                Debug.Log($"AppStorePurchaseManager: Purchase {productId} succeeded");
                TriggerPurchaseSuccess(product);
            }
            else
            {
                TriggerPurchaseFailed(null, $"Product {productId} not found");
            }
        }
    }
}
