namespace Playbox.Purchases
{
    public interface IProduct
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        decimal Price { get; }
        string Currency { get; }
    }
}
