using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Playbox.Purchases
{
    public abstract class PurchaseManager : IPurchaseManager
    {
        protected List<IProduct> _products = new List<IProduct>();

        private const string FolderPath = "Assets/Playbox/PurchaseProduct";
        private const string FileName = "Products.json";
        private string _filePath => Path.Combine(FolderPath, FileName);

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

        public event System.Action<IProduct> OnPurchaseSuccess;
        public event System.Action<IProduct, string> OnPurchaseFailed;

        protected void TriggerPurchaseSuccess(IProduct product) => OnPurchaseSuccess?.Invoke(product);
        protected void TriggerPurchaseFailed(IProduct product, string reason) => OnPurchaseFailed?.Invoke(product, reason);

        private void LoadProductsFromJSON()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
                Debug.Log($"Purchase folder created at {FolderPath}");
            }

            if (!File.Exists(_filePath))
            {
                Debug.LogWarning($"Products.json not found at {_filePath}");
                return;
            }

            string json = File.ReadAllText(_filePath);
            var defs = JsonConvert.DeserializeObject<List<ProductJson>>(json);

            _products.Clear();
            foreach (var def in defs)
            {
                _products.Add(new RuntimeProduct(def));
            }

            Debug.Log($"Loaded {_products.Count} products from JSON");
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

        private bool IsAlreadyPurchased(string productId) => LoadNonConsumables().Contains(productId);

        public bool IsProductPurchased(string productId)
        {
            return IsAlreadyPurchased(productId);
        }

        protected void AddConsumable(string productId, int amount)
        {
            int current = PlayerPrefs.GetInt(productId, 0);
            PlayerPrefs.SetInt(productId, current + amount);
            PlayerPrefs.Save();
        }

        public int GetConsumableAmount(string productId)
        {
            return PlayerPrefs.GetInt(productId, 0);
        }
    }
}