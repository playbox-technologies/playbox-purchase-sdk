using UnityEngine;

namespace Playbox.Purchases
{
    public class TestPurchaseManager : PurchaseManager
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("<color=yellow>[TestPurchaseManager] Initialized (fake IAP)</color>");
        }

        protected override void ProcessPlatformPurchase(IProduct product)
        {
            if (product == null)
            {
                Debug.LogWarning("[TestPurchaseManager] Product is null in ProcessPlatformPurchase");
                return;
            }

            Debug.Log($"<color=yellow>[TestPurchaseManager] Simulating purchase of {product.Id}</color>");

            if (product.Type == ProductType.NonConsumable)
            {
                SaveNonConsumable(product.Id);
                Debug.Log($"[TestPurchaseManager] Saved non-consumable {product.Id}");
            }
            else if (product.Type == ProductType.Consumable)
            {
                AddConsumable(product.Id, 1);
                Debug.Log($"[TestPurchaseManager] Added consumable {product.Id} x1");
            }

            TriggerPurchaseSuccess(product);

            Debug.Log($"<color=green>[TestPurchaseManager] Purchase SUCCESS {product.Id}</color>");
        }
    }
}