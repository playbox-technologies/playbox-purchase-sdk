namespace Playbox.Purchases
{
    public static class DependencyResolver
    {
        private static IPurchaseManager _purchaseManager;

        public static void Initialize()
        {
#if UNITY_ANDROID
            _purchaseManager = new GooglePlayPurchaseManager();
#elif UNITY_IOS
            //_purchaseManager = new AppStorePurchaseManager();
#else
           // _purchaseManager = new TestPurchaseManager();
#endif
            _purchaseManager.Initialize();
        }

        public static IPurchaseManager GetPurchaseManager()
        {
            if (_purchaseManager == null)
                Initialize();

            return _purchaseManager;
        }
    }
}
