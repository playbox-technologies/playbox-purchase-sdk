namespace Playbox.Purchases
{
    public interface IPurchaseManager
    {
        void Initialize();
        IProduct[] GetProducts();
        void Purchase(string productId);

        event System.Action<IProduct> OnPurchaseSuccess;

        event System.Action<IProduct, string> OnPurchaseFailed;
    }
}
