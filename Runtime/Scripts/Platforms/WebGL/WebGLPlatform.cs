#if UNITY_WEBGL
using System.Runtime.InteropServices;
using UnityEngine;
using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;

namespace Appcharge.PaymentLinks.Platforms.WebGL {
    public class WebGLPlatform : ICheckoutPlatform
    {
        [DllImport("__Internal")]
        private static extern void AC_LoadCore();

        [DllImport("__Internal")]
        private static extern void AC_Init(string sdkVersion);

        [DllImport("__Internal")]
        private static extern void AC_OpenCheckout(string purchaseId, string parsedUrl, string customerId);

        private WebGLEventHandler _webGLEventHandler;
        public ICheckoutPurchase Callback { get; set; }

        public WebGLPlatform() {
        }

        public static void LoadRemoteLib() {
            AC_LoadCore();
        }

        public void Init(ICheckoutPurchase callback) => InitInternal(callback);

        public void Init(string checkoutToken, string environment, ICheckoutPurchase callback) =>
            InitInternal(callback);

        private void InitInternal(ICheckoutPurchase callback)
        {
            Callback = callback;
            InitEventHandler(callback);
            AC_Init(SdkVersion.UnitySdkVersion);
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
