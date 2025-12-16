namespace Playbox.Purchases
{
    /// <summary>
    /// Represents a product in JSON format for deserialization.
    /// </summary>
    [System.Serializable]
    public class ProductJson
    {
        public string Id = "new_id";
        public string Name = "New Product";
        public string Description = "Description";
        public double Price = 0.99;
        public string Currency = "USD";
        public int Amount = 1;
        public ProductType Type;
    }
}