#if UNITY_WEBGL
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using Appcharge.PaymentLinks.Config;

namespace Appcharge.PaymentLinks.Platforms.WebGL {
    public class WebGLPlatform : ICheckoutPlatform
    {
        [DllImport("__Internal")]
        private static extern void AC_Init(string checkoutKey, string environment, string customerId);

        [DllImport("__Internal")]
        private static extern void AC_OpenCheckout(string sessionUrl, string sessionToken, string purchaseId);

        [DllImport("__Internal")]
        private static extern void AC_GetPricePoints();

        [DllImport("__Internal")]
        private static extern IntPtr AC_GetSDKVersion();

        private WebGLEventHandler _webGLEventHandler;
        private AppchargeConfig _editorConfig;
        public ICheckoutPurchase Callback { get; set; }

        public void Init(string checkoutPublicKey, string environment, string customerId, ICheckoutPurchase callback)
        {
            Initialize(checkoutPublicKey, environment.ToString().ToLower(), customerId, callback);

            if (!SetConfig()) {
                Debug.LogError("AppchargeConfig not found.");
                return;
             }
        }

        public void Init(string customerId, ICheckoutPurchase callback)
        {
             if (!SetConfig()) {
                Debug.LogError("AppchargeConfig not found.");
                return;
             }
            
            var config = new ConfigModel
            {
                checkoutPublicKey = _editorConfig.CheckoutPublicKey,
                environment = _editorConfig.Environment.ToString().ToLower(),
            };
            
            Initialize(config.checkoutPublicKey, config.environment.ToString().ToLower(), customerId, callback);
        }

        public void Initialize(string checkoutKey, string environment, string customerId, ICheckoutPurchase callback)
        {
            InitEventHandler(callback);
            AC_Init(checkoutKey, environment, customerId);
        }

        private void InitEventHandler(ICheckoutPurchase callback) {
            if (_webGLEventHandler)
                return;

            GameObject eventReceiverObject = new GameObject("WebGLEventHandler");
            _webGLEventHandler = eventReceiverObject.AddComponent<WebGLEventHandler>();
            _webGLEventHandler.Inject(callback);
        }

        public void OpenCheckout(string url, string sessionToken , string purchaseId) {
            AC_OpenCheckout(url, sessionToken, purchaseId);
        }

        public void GetPricePoints()
        {
            AC_GetPricePoints();
        }

        public string GetSdkVersion()
        {
            IntPtr resultPtr = AC_GetSDKVersion();
            string result = Marshal.PtrToStringUTF8(resultPtr);
            return result;
        }

        public void OpenSubscriptionManager(string url)
        {
            Debug.LogError("OpenSubscriptionManager is not implemented for WebGL.");
        }

        public void ConfigurePlatform(string property, object value)
        {
        }

        private bool SetConfig() {
            _editorConfig = ConfigUtility.GetConfig();
            return _editorConfig != null;
        }
    }
}
#endif