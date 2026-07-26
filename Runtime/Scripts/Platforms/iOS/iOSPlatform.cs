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
        private static extern void acbridge_initialize(string configJson);

        [DllImport("__Internal")]
        private static extern void acbridge_openCheckout(string purchaseId, string parsedUrl, string customerId);

        [DllImport("__Internal")]
        private static extern void acbridge_handleDeepLink(string url);

        [DllImport("__Internal")]
        private static extern string acbridge_getSdkVersion();

        [DllImport("__Internal")]
        private static extern void acbridge_getPricePoints();

        [DllImport("__Internal")]
        private static extern void acbridge_setPortraitOrientationLock(bool portraitOrientationLock);

        [DllImport("__Internal")]
        private static extern void acbridge_setDebugMode(bool debugMode);

        [DllImport("__Internal")]
        private static extern void acbridge_setBrowserMode(string mode);
#else
        // Stub implementations when not on iOS
        private static void acbridge_initialize(string configJson) { }
        private static void acbridge_openCheckout(string purchaseId, string parsedUrl, string customerId) { }
        private static void acbridge_handleDeepLink(string url) { }
        private static string acbridge_getSdkVersion() { return SdkVersion.UnitySdkVersion; }
        private static void acbridge_getPricePoints() { }
        private static void acbridge_setPortraitOrientationLock(bool portraitOrientationLock) { }
        private static void acbridge_setDebugMode(bool debugMode) { }
        private static void acbridge_setBrowserMode(string mode) { }
#endif
        private bool _eventHandlerInitialized = false;
        private BrowserMode _browserMode = BrowserMode.Internal;
        private bool _portraitOrientationLock = false;
        private bool _debugMode = false;
        private string _applinksDomain = null;
        public ICheckoutPurchase Callback { get; set; }

        public void Init(ICheckoutPurchase callback)
        {
            AppchargeConfig editorConfig = ConfigUtility.GetConfig();
            
            if (editorConfig == null) {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }
            
            Init(editorConfig.CheckoutPublicKey, editorConfig.Environment.ToString().ToLowerInvariant(), callback);
        }

        public void Init(string checkoutToken, string environment, ICheckoutPurchase callback) {            
            AppchargeConfig editorConfig = ConfigUtility.GetConfig();
            
            if (editorConfig == null) {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }
            
            _browserMode = editorConfig.BrowserMode;
            _debugMode = editorConfig.EnableDebugMode;
            _portraitOrientationLock = editorConfig.PortraitOrientationLock;    
            _applinksDomain = editorConfig.AssociatedDomain;

            InternalInit(checkoutToken, environment, callback, _applinksDomain, _browserMode == BrowserMode.Internal);   
            SetPortraitOrientationLock(_portraitOrientationLock);
        }

        private void InternalInit(string checkoutPublicKey, string environment, ICheckoutPurchase callback, string applinksDomain, bool useSFSVC)
        {
            Callback = callback;
            InitEventHandler();

            string redirectUrl = applinksDomain;
            
            if (!redirectUrl.StartsWith("https://") && !string.IsNullOrEmpty(redirectUrl))
                redirectUrl = "https://" + redirectUrl;

            ConfigModel configModel = new ConfigModel
            {
                checkoutPublicKey = checkoutPublicKey,
                environment = environment,
                redirectUrl = redirectUrl,
            };

            acbridge_initialize(configModel.ToJson()); 
        }

        private void InitEventHandler() 
        {
            if (_eventHandlerInitialized && _nativeCallbackHandler != null && _nativeCallbackHandler.gameObject != null)
            {
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
                _nativeCallbackHandler.Inject(this);
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

        public void OpenCheckout(string purchaseId, string parsedUrl, string customerId)
        {
            acbridge_openCheckout(purchaseId, parsedUrl, customerId);
        }

        public void HandleDeepLink(string url) {
            acbridge_handleDeepLink(url);
        }

        public string GetSdkVersion() {
            return acbridge_getSdkVersion();
        }
                                   
        public void GetPricePoints() {
            acbridge_getPricePoints();
        }

        public void ConfigurePlatform(string property, object value) {
           if (property.Equals("browserMode") && value is BrowserMode)
           {
               SetBrowserMode((BrowserMode)value);
           }

           if (property.Equals("portraitOrientationLock") && value is bool)
           {
               SetPortraitOrientationLock((bool)value);
           }

           if (property.Equals("debugMode") && value is bool)
           {
               SetDebugMode((bool)value);
           }
        }

        private void SetPortraitOrientationLock(bool portraitOrientationLock) {
            _portraitOrientationLock = portraitOrientationLock;
            acbridge_setPortraitOrientationLock(portraitOrientationLock);
        }

        private void SetBrowserMode(BrowserMode mode) {
            _browserMode = mode;
            acbridge_setBrowserMode(mode.ToString());
        }            

        private void SetDebugMode(bool debugMode) {
            _debugMode = debugMode;
            acbridge_setDebugMode(debugMode);
        }

        public void OnInitialized() {
            SetDebugMode(_debugMode);
            SetBrowserMode(_browserMode);
            SetPortraitOrientationLock(_portraitOrientationLock);
        }
    }
}