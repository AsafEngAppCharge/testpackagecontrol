using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.Base {
    public abstract class BaseSdkVersion
    {
        protected ICheckoutPlatform _platform;

        public BaseSdkVersion(ICheckoutPlatform platform)
        {
            _platform = platform;
        }

        public abstract string GetSdkVersion();
    }
}
