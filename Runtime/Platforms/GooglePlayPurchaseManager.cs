using UnityEngine;

namespace Playbox.Purchases
{
    public class GooglePlayPurchaseManager : BasePurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("GooglePlayPurchaseManager Initialized");

            //test
            _products.Add(new Product("coin_pack_1", "100 Coins", "Pack of 100 coins", 0.99m, "USD"));
            _products.Add(new Product("coin_pack_2", "500 Coins", "Pack of 500 coins", 3.99m, "USD"));
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
            {
                TriggerPurchaseFailed(null, $"Product {productId} not found");
            }
        }
    }

}