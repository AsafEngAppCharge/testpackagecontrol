#if UNITY_WEBGL
using System.Runtime.InteropServices;
using UnityEngine;
using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.WebGL {
    public class WebGLPlatform : ICheckoutPlatform
    {
        [DllImport("__Internal")]
        private static extern void AC_Init(string sdkVersion, string coreScriptSrc);

        [DllImport("__Internal")]
        private static extern void AC_OpenCheckout(string purchaseId, string parsedUrl, string customerId);

        private WebGLEventHandler _webGLEventHandler;
        public ICheckoutPurchase Callback { get; set; }

        public void Init(ICheckoutPurchase callback)
        {
            AppchargeConfig config = ConfigUtility.GetConfig();
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
            AC_Init(SdkVersion.UnitySdkVersion, WebGLCoreData.Resolve(environment));
        }

        private void InitEventHandler(ICheckoutPurchase callback) {
            if (_webGLEventHandler) {
                _webGLEventHandler.Inject(callback);
                return;
            }

            GameObject eventReceiverObject = new GameObject("WebGLEventHandler");
            _webGLEventHandler = eventReceiverObject.AddComponent<WebGLEventHandler>();
            _webGLEventHandler.Inject(callback);
        }

        public void OpenCheckout(string purchaseId, string parsedUrl, string customerId)
        {
            _webGLEventHandler?.SetCheckoutData(purchaseId, parsedUrl, customerId);
            AC_OpenCheckout(purchaseId, parsedUrl, customerId);
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
