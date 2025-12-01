using UnityEngine;

namespace Playbox.Purchases
{

    public class TestPurchaseManager : BasePurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("TestPurchaseManager Initialized");

            _products.Add(new Product("test_product_1", "Test Product 1", "Description for product 1", 0.0m, "USD"));
            _products.Add(new Product("test_product_2", "Test Product 2", "Description for product 2", 0.0m, "USD"));
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
