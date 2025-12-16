namespace Playbox.Purchases
{
    /// <summary>
    /// Represents a product that is created at runtime from a ProductJson definition.
    /// </summary>
    public class RuntimeProduct : IProduct
    {
        readonly private ProductJson _def;

        public RuntimeProduct(ProductJson def) => _def = def;
        public string Id => _def.Id;
        public string Name => _def.Name;
        public string Description => _def.Description;
        public decimal Price => (decimal)_def.Price;
        public string Currency => _def.Currency;
        public int Amount => _def.Amount;
        public ProductType Type => _def.Type;
    }
}
