using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.Base {
    public abstract class BaseInit
    {
        protected ICheckoutPlatform _platform;

        public BaseInit(ICheckoutPlatform platform)
        {
            _platform = platform;
        }

        public abstract void Initialize(string customerId, ICheckoutPurchase callback);
        public abstract void Initialize(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback);
    }
}
