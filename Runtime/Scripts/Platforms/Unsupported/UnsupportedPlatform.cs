using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Unsupported {
    public class UnsupportedPlatform : ICheckoutPlatform
    {
        public ICheckoutPurchase Callback { get; set; }

        public void Init(string customerId, ICheckoutPurchase callback)
        {
            Debug.LogWarning("Unsupported platform: Init");        
            callback.OnInitializeFailed(new ErrorMessage { message = "Unsupported platform" });
        }

        public void Init(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback)
        {
            Debug.LogWarning("Unsupported platform: Init");        
            callback.OnInitializeFailed(new ErrorMessage { message = "Unsupported platform" });
        }

        public void OpenCheckout(string url, string sessionToken, string purchaseId)
        {
            Debug.LogWarning("Unsupported platform: OpenCheckout");        
        }
        
        public string GetSdkVersion()
        {
            Debug.LogWarning("Unsupported platform: GetSdkVersion");
            return "0.0.0";
        }

        
        public void GetPricePoints()
        {
            Debug.LogWarning("Unsupported platform: GetPricePoints");        
        }
        
        public void OpenSubscriptionManager(string url)
        {
            Debug.LogWarning("Unsupported platform: OpenSubscriptionManager");        
        }

        public void ConfigurePlatform(string property, object value)
        {
            Debug.LogWarning("Unsupported platform: ConfigurePlatform");        
        }
    }
}
