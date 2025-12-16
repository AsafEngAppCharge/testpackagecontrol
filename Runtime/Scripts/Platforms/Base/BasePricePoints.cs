using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.Base {
    public abstract class BasePricePoints
    {
        protected ICheckoutPlatform _platform;

        public BasePricePoints(ICheckoutPlatform platform)
        {
            _platform = platform;
        }

        public abstract void GetPricePoints();
    }
}
