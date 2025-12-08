using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Playbox.Purchases
{
    public abstract class PurchaseManager : IPurchaseManager
    {
        protected List<IProduct> _products = new List<IProduct>();

        private const string ProductsResourcesPath = "IAP/Products";

        private const string NonConsumablesKey = "NonConsumablePurchases";

        public virtual void Initialize()
        {
            LoadProductsFromJSON();
        }

        public virtual IProduct[] GetProducts() => _products.ToArray();

        protected abstract void ProcessPlatformPurchase(IProduct product);

        public void Purchase(string productId)
        {
            var product = _products.Find(p => p.Id == productId);
            if (product == null)
            {
                TriggerPurchaseFailed(null, "Product not found");
                return;
            }

            if (product.Type == ProductType.NonConsumable && IsAlreadyPurchased(productId))
            {
                TriggerPurchaseFailed(product, "Already purchased");
                return;
            }

            ProcessPlatformPurchase(product);
        }

        public event Action<IProduct> OnPurchaseSuccess;
        public event Action<IProduct, string> OnPurchaseFailed;

        protected void TriggerPurchaseSuccess(IProduct product) =>
            OnPurchaseSuccess?.Invoke(product);

        protected void TriggerPurchaseFailed(IProduct product, string reason) =>
            OnPurchaseFailed?.Invoke(product, reason);

        private void LoadProductsFromJSON()
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>(ProductsResourcesPath);
                if (textAsset == null)
                {
                    Debug.LogWarning($"[IAP] Products.json not found in Resources at path '{ProductsResourcesPath}'");
                    return;
                }

                string json = textAsset.text;
                var defs = JsonConvert.DeserializeObject<List<ProductJson>>(json);

                if (defs == null)
                {
                    Debug.LogError("[IAP] Failed to deserialize products.json: defs == null");
                    return;
                }

                _products.Clear();
                foreach (var def in defs)
                {
                    if (def == null)
                        continue;

                    _products.Add(new RuntimeProduct(def));
                }

                Debug.Log($"[IAP] Loaded {_products.Count} products from JSON (Resources)");
            }
            catch (Exception e)
            {
                Debug.LogError("[IAP] Exception in LoadProductsFromJSON: " + e);
            }
        }

        protected void SaveNonConsumable(string productId)
        {
            var purchases = LoadNonConsumables();
            if (!purchases.Contains(productId))
            {
                purchases.Add(productId);
                PlayerPrefs.SetString(NonConsumablesKey, JsonConvert.SerializeObject(purchases));
                PlayerPrefs.Save();
            }
        }

        private List<string> LoadNonConsumables()
        {
            if (!PlayerPrefs.HasKey(NonConsumablesKey))
                return new List<string>();

            return JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString(NonConsumablesKey));
        }

        private bool IsAlreadyPurchased(string productId) =>
            LoadNonConsumables().Contains(productId);

        public bool IsProductPurchased(string productId) =>
            IsAlreadyPurchased(productId);

        protected void AddConsumable(string productId, int amount)
        {
            int current = PlayerPrefs.GetInt(productId, 0);
            PlayerPrefs.SetInt(productId, current + amount);
            PlayerPrefs.Save();
        }

        public int GetConsumableAmount(string productId) =>
            PlayerPrefs.GetInt(productId, 0);
    }
}