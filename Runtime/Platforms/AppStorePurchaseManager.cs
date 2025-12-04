using UnityEngine;

namespace Playbox.Purchases
{
    public class AppStorePurchaseManager : PurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("AppStorePurchaseManager Initialized");
        }

        // TODO: Implement platform-specific purchase logic here
        protected override bool ProcessPlatformPurchase(IProduct product)
        {
            Debug.Log($"AppStorePurchaseManager: Processing purchase {product.Id}");
            return true;
        }
    }
}