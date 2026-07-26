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
        private static extern void AC_OpenCheckout(string purchaseId, string parsedUrl, string customerId);

        [DllImport("__Internal")]
        private static extern void AC_GetPricePoints();

        private WebGLEventHandler _webGLEventHandler;
        public ICheckoutPurchase Callback { get; set; }

        public WebGLPlatform() {
        }

        public static void LoadRemoteLib() {
            AC_LoadCore();
        }

        public void Init(ICheckoutPurchase callback)
        {
            var config = ConfigUtility.GetConfig();
            if (config == null)
            {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }

            Init(config.CheckoutPublicKey, config.Environment.ToString().ToLowerInvariant(), callback);
        }

        public void Init(string checkoutToken, string environment, ICheckoutPurchase callback)
        {
            Callback = callback;
            InitEventHandler(callback);
            AC_Init(SdkVersion.UnitySdkVersion, checkoutToken);
        }

        private void InitEventHandler(ICheckoutPurchase callback) {
            if (_webGLEventHandler) {
                return;
            }

            GameObject eventReceiverObject = new GameObject("WebGLEventHandler");
            _webGLEventHandler = eventReceiverObject.AddComponent<WebGLEventHandler>();
            _webGLEventHandler.Inject(callback);
        }

        public void OpenCheckout(string purchaseId, string parsedUrl, string customerId)
        {
            AC_OpenCheckout(purchaseId, parsedUrl, customerId);
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
