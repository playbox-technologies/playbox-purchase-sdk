using UnityEngine;

namespace Playbox.Purchases
{
    public class TestPurchaseManager : BasePurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("TestPurchaseManager Initialized");
        }

        public override void Purchase(string productId)
        {
            var product = _products.Find(p => p.Id == productId);
            if (product != null)
            {
                Debug.Log($"TestPurchaseManager: Purchase {productId} succeeded (simulation)");
                TriggerPurchaseSuccess(product);
            }
            else
            {
                TriggerPurchaseFailed(null, $"Test product {productId} not found");
            }
        }
    }
}
