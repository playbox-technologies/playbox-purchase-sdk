using System.Collections.Generic;
using UnityEngine;

namespace Playbox.Purchases
{
    public abstract class BasePurchaseManager : IPurchaseManager
    {
        protected List<IProduct> _products = new List<IProduct>();

        public virtual void Initialize() { }

        public virtual IProduct[] GetProducts() => _products.ToArray();

        public abstract void Purchase(string productId);

        public event System.Action<IProduct> OnPurchaseSuccess;
        public event System.Action<IProduct, string> OnPurchaseFailed;

        protected void TriggerPurchaseSuccess(IProduct product) => OnPurchaseSuccess?.Invoke(product);

        protected void TriggerPurchaseFailed(IProduct product, string reason) => OnPurchaseFailed?.Invoke(product, reason);
    }
}
