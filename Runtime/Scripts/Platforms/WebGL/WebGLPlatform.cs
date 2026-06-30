#if UNITY_WEBGL
using System.Runtime.InteropServices;
using UnityEngine;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Config;

namespace Appcharge.PaymentLinks.Platforms.WebGL {
    public class WebGLPlatform : ICheckoutPlatform
    {
        [DllImport("__Internal")]
        private static extern void AC_LoadCore();

        [DllImport("__Internal")]
        private static extern void AC_Init(string sdkVersion, string checkoutPublicKey);

        [DllImport("__Internal")]
        private static extern void AC_OpenCheckout(string purchaseId, string parsedUrl);

        [DllImport("__Internal")]
        private static extern void AC_OpenCheckoutLegacy(string sessionUrl, string sessionToken, string purchaseId);

        [DllImport("__Internal")]
        private static extern void AC_GetPricePoints();

        private WebGLEventHandler _webGLEventHandler;
        public ICheckoutPurchase Callback { get; set; }

        public WebGLPlatform() {

        }

        public static void LoadRemoteLib() {
            AC_LoadCore();
        }

        public void Init(string checkoutPublicKey, string environment, string customerId, ICheckoutPurchase callback)
        {
            Initialize(checkoutPublicKey, callback);
        }

        public void Init(string customerId, ICheckoutPurchase callback)
        {
            Initialize(ConfigUtility.GetConfig().CheckoutPublicKey, callback);
        }

        private void Initialize(string checkoutPublicKey, ICheckoutPurchase callback)
        {
            InitEventHandler(callback);
            AC_Init(SdkVersion.UnitySdkVersion, checkoutPublicKey);
        }

        private void InitEventHandler(ICheckoutPurchase callback) {
            if (_webGLEventHandler) {
                return;
            }

            GameObject eventReceiverObject = new GameObject("WebGLEventHandler");
            _webGLEventHandler = eventReceiverObject.AddComponent<WebGLEventHandler>();
            _webGLEventHandler.Inject(callback);
        }

        public void OpenCheckout(string url, string sessionToken , string purchaseId) {
            AC_OpenCheckoutLegacy(url, sessionToken, purchaseId);
        }

        public void OpenCheckout(string purchaseId, string parsedUrl)
        {
            AC_OpenCheckout(purchaseId, parsedUrl);
        }

        public void GetPricePoints()
        {
            AC_GetPricePoints();
        }

        public string GetSdkVersion()
        {
            return SdkVersion.UnitySdkVersion;
        }

        public void ConfigurePlatform(string property, object value)
        {
        }
    }
}
#endif