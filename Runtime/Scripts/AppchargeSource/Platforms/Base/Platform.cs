using System.Collections.Generic;
using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.Base {
    public abstract class Platform : ICheckoutPlatform
    {
        protected BaseInit _init;
        protected BaseOpenCheckout _openCheckout;
        protected BaseSdkVersion _sdkVersion;
        protected BasePricePoints _pricePoints;
        protected BaseSubscription _subscription;
        public ICheckoutPurchase Callback { get; set; }
        protected abstract void InitializeComponents();

        public void Init(string customerId, ICheckoutPurchase callback)
        {
            this.Callback = callback;
            InitializeComponents();
            _init.Initialize(customerId, callback);
        }
        
        public void Init(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback)
        {
            this.Callback = callback;
            InitializeComponents();
            _init.Initialize(checkoutToken, environment, customerId, callback);
        }
        
        public void OpenCheckout(string url, string sessionToken, string purchaseId)
        {
            _openCheckout.OpenCheckout(url, sessionToken, purchaseId);
        }
        
        public string GetSdkVersion()
        {
            return _sdkVersion.GetSdkVersion();
        }
        
        public void GetPricePoints()
        {
            _pricePoints.GetPricePoints();
        }
        
        public void OpenSubscriptionManager(string url)
        {
            _subscription.OpenSubscriptionManager(url);
        }

        public abstract void ConfigurePlatform(string property, object value);
    }
}
