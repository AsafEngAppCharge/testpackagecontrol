using System.Collections.Generic;

namespace Appcharge.PaymentLinks.Interfaces {
    public interface ICheckoutPlatform
    {
        ICheckoutPurchase Callback { get; set; }
        void Init(string customerId, ICheckoutPurchase callback);
        void Init(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback);
        void OpenCheckout(string url, string sessionToken, string purchaseId);
        string GetSdkVersion();
        void GetPricePoints();
        void OpenSubscriptionManager(string url);
        void ConfigurePlatform(string property, object value);
    }
}
