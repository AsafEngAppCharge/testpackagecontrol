using System.Collections.Generic;
using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.Base {
    public abstract class Platform : ICheckoutPlatform
    {
        protected BaseInit _init;
        protected BaseOpenCheckout _openCheckout;
        protected BaseSdkVersion _sdkVersion;
        public ICheckoutPurchase Callback { get; set; }
        protected abstract void InitializeComponents();

        public void Init(ICheckoutPurchase callback)
        {
            this.Callback = callback;
            InitializeComponents();
            _init.Initialize(callback);
        }
        
        public void Init(string checkoutToken, string environment, ICheckoutPurchase callback)
        {
            this.Callback = callback;
            InitializeComponents();
            _init.Initialize(checkoutToken, environment, callback);
        }
        
        public void OpenCheckout(string purchaseId, string parsedUrl, string customerId)
        {
            _openCheckout.OpenCheckout(purchaseId, parsedUrl, customerId);
        }
        
        public string GetSdkVersion()
        {
            return _sdkVersion.GetSdkVersion();
        }

        public abstract void ConfigurePlatform(string property, object value);
    }
}
