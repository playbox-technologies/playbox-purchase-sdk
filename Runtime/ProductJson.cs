using UnityEngine;

namespace Playbox.Purchases
{
    [System.Serializable]
    public class ProductJson
    {
        public string Id = "new_id";
        public string Name = "New Product";
        public string Description = "Description";
        public double Price = 0.99;
        public string Currency = "USD";
        public ProductType Type;
    }
}
