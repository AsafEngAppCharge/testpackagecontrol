namespace Appcharge.PaymentLinks.Interfaces {
    public interface ICheckoutPlatform
    {
        ICheckoutPurchase Callback { get; set; }
        void Init(ICheckoutPurchase callback);
        void Init(string checkoutToken, string environment, ICheckoutPurchase callback);
        void OpenCheckout(string purchaseId, string parsedUrl, string customerId);
        string GetSdkVersion();
        void ConfigurePlatform(string property, object value);
    }
}
