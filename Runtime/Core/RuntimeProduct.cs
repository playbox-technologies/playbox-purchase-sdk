namespace Playbox.Purchases
{
    public class RuntimeProduct : IProduct
    {
        readonly private ProductJson _def;

        public RuntimeProduct(ProductJson def) => _def = def;
        public string Id => _def.Id;
        public string Name => _def.Name;
        public string Description => _def.Description;
        public decimal Price => (decimal)_def.Price;
        public string Currency => _def.Currency;
        public ProductType Type => _def.Type;
    }
}
