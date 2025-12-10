namespace Playbox.Purchases
{
    public static class DependencyResolver
    {
        private static PurchaseManager _purchaseManager;

        public static void Initialize()
        {
            _purchaseManager = new UniversalPurchaseManager();
            _purchaseManager.Initialize();
        }

        public static PurchaseManager GetPurchaseManager()
        {
            if (_purchaseManager == null)
                Initialize();

            return _purchaseManager;
        }
    }
}
