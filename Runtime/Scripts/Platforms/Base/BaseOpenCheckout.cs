using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.Base {
    public abstract class BaseOpenCheckout
    {
        protected ICheckoutPlatform _platform;

        public BaseOpenCheckout(ICheckoutPlatform platform)
        {
            _platform = platform;
        }

        public abstract void OpenCheckout(string url, string sessionToken, string purchaseId);
    }
}
