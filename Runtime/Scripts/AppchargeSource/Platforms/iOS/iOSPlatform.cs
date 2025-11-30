#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.iOS {
    public class iOSPlatform : ICheckoutPlatform {
        private static NativeiOSCallbackHandler _nativeCallbackHandler;
        
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void acbridge_initialize(string configJson, string customerId);

        [DllImport("__Internal")]
        private static extern void acbridge_openCheckout(string sessionToken, string purchaseId, string url);

        [DllImport("__Internal")]
        private static extern void acbridge_handleDeepLink(string url);

        [DllImport("__Internal")]
        private static extern string acbridge_getSdkVersion();

        [DllImport("__Internal")]
        private static extern void acbridge_setUseExternalBrowser(bool useExternal);

        [DllImport("__Internal")]
        private static extern void acbridge_getPricePoints();

        [DllImport("__Internal")]
        private static extern void acbridge_openSubscriptionManager(string url);

        [DllImport("__Internal")]
        private static extern void acbridge_setPortraitOrientationLock(bool portraitOrientationLock);
#else
        // Stub implementations when not on iOS
        private static void acbridge_initialize(string configJson, string customerId) { }
        private static void acbridge_openCheckout(string sessionToken, string purchaseId, string url) { }
        private static void acbridge_handleDeepLink(string url) { }
        private static string acbridge_getSdkVersion() { return "0.0.0"; }
        private static void acbridge_setUseExternalBrowser(bool useExternal) { }
        private static void acbridge_getPricePoints() { }
        private static void acbridge_openSubscriptionManager(string url) { }
        private static void acbridge_setPortraitOrientationLock(bool portraitOrientationLock) { }
#endif
        private bool _eventHandlerInitialized = false;
        private bool _useInternalBrowser = true;
        private bool _portraitOrientationLock = false;
        private string _applinksDomain = null;
        private string _customerId = null;
        public ICheckoutPurchase Callback { get; set; }
        private ConfigModel _config;

        public void Init(string customerId, ICheckoutPurchase callback)
        {
            AppchargeConfig editorConfig = ConfigUtility.GetConfig();
            
            if (editorConfig == null) {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }
            
            Init(editorConfig.CheckoutPublicKey, editorConfig.Environment.ToString().ToLowerInvariant(), 
                customerId, callback);
        }

        public void Init(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback) {
            InternalInit(checkoutToken, environment, customerId, callback, _applinksDomain, _useInternalBrowser);   
            
            AppchargeConfig editorConfig = ConfigUtility.GetConfig();
            
            if (editorConfig == null) {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }
            
            _useInternalBrowser = editorConfig.UseInternalBrowser;
            _portraitOrientationLock = editorConfig.PortraitOrientationLock;    
            _applinksDomain = editorConfig.AssociatedDomain;

            SetUseExternalBrowser(!_useInternalBrowser);
            SetPortraitOrientationLock(_portraitOrientationLock);
        }

        private void InternalInit(string checkoutPublicKey, string environment, string customerId, ICheckoutPurchase callback, string applinksDomain, bool useInternalBrowser)
        {
            _customerId = customerId;
            Callback = callback;
            InitEventHandler(callback);

            string redirectUrl = null;
            if (!useInternalBrowser && !string.IsNullOrEmpty(applinksDomain))
            {
                redirectUrl = applinksDomain;
                if (!redirectUrl.StartsWith("https://"))
                {
                    redirectUrl = "https://" + redirectUrl;
                }
            }   

            ConfigModel configModel = new ConfigModel
            {
                checkoutPublicKey = checkoutPublicKey,
                environment = environment,
                redirectUrl = redirectUrl
            };

            _config = configModel;
            
            acbridge_initialize(configModel.ToJson(), customerId); 
        }

        private void InitEventHandler(ICheckoutPurchase callback) 
        {
            if (_eventHandlerInitialized && _nativeCallbackHandler != null && _nativeCallbackHandler.gameObject != null)
            {
                _nativeCallbackHandler.Inject(callback);
                return;
            }

            if (_nativeCallbackHandler != null && _nativeCallbackHandler.gameObject == null)
            {
                _nativeCallbackHandler = null;
                _eventHandlerInitialized = false;
            }

            if (_nativeCallbackHandler == null)
            {
                GameObject eventReceiverObject = new GameObject("ACCallbackHandler");
                eventReceiverObject.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(eventReceiverObject);
                _nativeCallbackHandler = eventReceiverObject.AddComponent<NativeiOSCallbackHandler>();
                _nativeCallbackHandler.Inject(callback);
            }

            if (!_eventHandlerInitialized)
            {
                try
                {
                    Application.deepLinkActivated += OnDeepLinkActivated;
                    _eventHandlerInitialized = true;
                }
                catch (System.Exception)
                {
                    _eventHandlerInitialized = false;
                }
            }
        }

        private void OnDeepLinkActivated(string url)
        {
            HandleDeepLink(url);
        }

        public void OpenCheckout(string url, string sessionToken , string purchaseId)        
        {
            acbridge_openCheckout(sessionToken, purchaseId, url);
        }

        public void HandleDeepLink(string url) {
            acbridge_handleDeepLink(url);
        }

        public string GetSdkVersion() {
            return acbridge_getSdkVersion();
        }

        private void SetUseExternalBrowser(bool useExternal) {
            acbridge_setUseExternalBrowser(useExternal);
        }                                               

        public void GetPricePoints() {
            acbridge_getPricePoints();
        }

        public void OpenSubscriptionManager(string url) {
            acbridge_openSubscriptionManager(url);
        }

        public void ConfigurePlatform(string property, object value) {
           if (property.Equals("useInternalBrowser") && value is bool)
           {
               _useInternalBrowser = (bool)value;
               SetUseExternalBrowser(!_useInternalBrowser);
           }

           if (property.Equals("portraitOrientationLock") && value is bool)
           {
               _portraitOrientationLock = (bool)value;
               SetPortraitOrientationLock(_portraitOrientationLock);
           }
        }

        public void SetPortraitOrientationLock(bool portraitOrientationLock) {
            acbridge_setPortraitOrientationLock(portraitOrientationLock);
        }
    }
}