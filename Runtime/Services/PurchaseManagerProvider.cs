namespace Playbox.Purchases
{
    public static class PurchaseManagerProvider
    {
        private static PurchaseManager _purchaseManager;

        /// <summary>
        /// Initializes the PurchaseManager instance.
        /// </summary>
        public static void Initialize()
        {
            _purchaseManager = new UniversalPurchaseManager();
            _purchaseManager.Initialize();
        }

        /// <summary>
        /// Returns the instance of PurchaseManager. Initializes it if necessary.
        /// </summary>
        /// <returns>PurchaseManager instance</returns>
        public static PurchaseManager GetPurchaseManager()
        {
            if (_purchaseManager == null)
                Initialize();

            return _purchaseManager;
        }
    }
}