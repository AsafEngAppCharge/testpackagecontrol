using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.Base {
    public abstract class BaseSubscription
    {
        protected ICheckoutPlatform _platform;

        public BaseSubscription(ICheckoutPlatform platform)
        {
            _platform = platform;
        }

        public abstract void OpenSubscriptionManager(string url);
    }
}
