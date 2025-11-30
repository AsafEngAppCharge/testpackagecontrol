#if UNITY_EDITOR
namespace Appcharge.PaymentLinks.Platforms.Editor.Models {
    [System.Serializable]
    public class OrderValidationApiResponse
    {
        public long date;
        public string sessionId;
        public string bundleName;
        public string bundleId;
        public string bundleSKU;
        public Product[] products;
        public int totalSum;
        public string totalSumCurrency;
        public string userId;
        public string userCountry;
        public string paymentMethodName;
        public string orderId;
        public string reason;
        public string state;
    }

    [System.Serializable]
    public class Product
    {
        public string name;
        public string sku;
        public string amount;
    }
}
#endif
