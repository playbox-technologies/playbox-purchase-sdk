namespace Playbox.Purchases
{
    /// <summary>
    /// Represents a purchasable product with basic details such as ID, name, price, and type.
    /// </summary>
    public interface IProduct
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        decimal Price { get; }
        string Currency { get; }
        int Amount { get; }
        ProductType Type { get; }

    }
}
