using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Playbox.Purchases
{
    public abstract class BasePurchaseManager : IPurchaseManager
    {
        protected List<IProduct> _products = new List<IProduct>();

        private const string FolderPath = "Assets/Playbox/PurchaseProduct";
        private const string FileName = "Products.json";
        private string _filePath => Path.Combine(FolderPath, FileName);

        public virtual void Initialize()
        {
            LoadProductsFromJSON();
        }

        public virtual IProduct[] GetProducts() => _products.ToArray();

        public abstract void Purchase(string productId);

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
    }

    public class RuntimeProduct : IProduct
    {
        private ProductJson _def;

        public RuntimeProduct(ProductJson def)
        {
            _def = def;
        }

        public string Id => _def.Id;
        public string Name => _def.Name;
        public string Description => _def.Description;
        public decimal Price => (decimal)_def.Price;
        public string Currency => _def.Currency;
    }
}

