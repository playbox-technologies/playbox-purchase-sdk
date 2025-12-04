using UnityEngine;

namespace Playbox.Purchases
{
    public class GooglePlayPurchaseManager : PurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("GooglePlayPurchaseManager Initialized");
        }

        // TODO: Implement platform-specific purchase logic here
        protected override bool ProcessPlatformPurchase(IProduct product)
        {
            Debug.Log($"GooglePlayPurchaseManager: Processing purchase {product.Id}");
            return true; 
        }
    }
}