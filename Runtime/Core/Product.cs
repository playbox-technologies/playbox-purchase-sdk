namespace Playbox.Purchases
{
    public class Product : IProduct
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public string Currency { get; private set; }

        public Product(string id, string name, string description, decimal price, string currency)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Currency = currency;
        }
    }
}
