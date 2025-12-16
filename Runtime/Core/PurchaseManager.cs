using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Playbox.Purchases
{
    /// <summary>
    /// Abstract base class for managing in-app purchases. Handles product loading, purchase processing,
    /// and tracking of consumable and non-consumable products.
    /// </summary>
    public abstract class PurchaseManager
    {
        protected List<IProduct> _products = new List<IProduct>();

        private const string ProductsResourcesPath = "Playbox/IAP/Products";

        private const string NonConsumablesKey = "NonConsumablePurchases";

        private bool _isInitialized = false;

        /// <summary>
        /// Initializes the purchase manager and loads products from a JSON file.
        /// </summary>
        public virtual void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[IAP] Initialize was already called.");
                return;
            }

            _isInitialized = true;

            LoadProductsFromJSON();
        }

        /// <summary>
        /// Gets the list of products.
        /// </summary>
        public virtual IProduct[] GetProducts() => _products.ToArray();

        /// <summary>
        /// Abstract method for processing platform-specific purchases.
        /// </summary>
        protected abstract void ProcessPlatformPurchase(IProduct product);

        /// <summary>
        /// Initiates a purchase of a product by its ID. Checks if the product is already purchased.
        /// </summary>
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

        /// <summary>
        /// Gets the product type (Consumable or Non-Consumable).
        /// </summary>
        public ProductType GetProductType(string productId)
        {
            var product = _products.Find(p => p.Id == productId);
            if (product != null)
            {
                return product.Type;
            }
            return ProductType.Consumable;
        }

        /// <summary>
        /// Event triggered when a purchase is successful.
        /// </summary>
        public event Action<IProduct> OnPurchaseSuccess;

        /// <summary>
        /// Event triggered when a purchase fails.
        /// </summary>
        public event Action<IProduct, string> OnPurchaseFailed;

        /// <summary>
        /// Triggers the purchase success event.
        /// </summary>
        protected void TriggerPurchaseSuccess(IProduct product) =>
            OnPurchaseSuccess?.Invoke(product);

        /// <summary>
        /// Triggers the purchase failure event.
        /// </summary>
        protected void TriggerPurchaseFailed(IProduct product, string reason) =>
            OnPurchaseFailed?.Invoke(product, reason);

        /// <summary>
        /// Loads product data from a JSON file in Resources.
        /// </summary>
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

        /// <summary>
        /// Saves a non-consumable product as purchased in PlayerPrefs.
        /// </summary>
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

        /// <summary>
        /// Loads the list of previously purchased non-consumable products from PlayerPrefs.
        /// </summary>
        private List<string> LoadNonConsumables()
        {
            if (!PlayerPrefs.HasKey(NonConsumablesKey))
                return new List<string>();

            return JsonConvert.DeserializeObject<List<string>>(PlayerPrefs.GetString(NonConsumablesKey));
        }

        /// <summary>
        /// Checks if a product has already been purchased.
        /// </summary>
        public bool IsAlreadyPurchased(string productId) =>
            LoadNonConsumables().Contains(productId);

        /// <summary>
        /// Checks if a product is purchased (same as IsAlreadyPurchased).
        /// </summary>
        public bool IsProductPurchased(string productId) =>
            IsAlreadyPurchased(productId);
    }
}