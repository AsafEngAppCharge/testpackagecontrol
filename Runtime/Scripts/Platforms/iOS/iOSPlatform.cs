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
        private const string UNITY_SDK_VERSION = "2.2.0";
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void acbridge_initialize(string configJson, string customerId, string platformIntegrationVersion);

        [DllImport("__Internal")]
        private static extern void acbridge_openCheckout(string sessionToken, string purchaseId, string url);

        [DllImport("__Internal")]
        private static extern void acbridge_handleDeepLink(string url);

        [DllImport("__Internal")]
        private static extern string acbridge_getSdkVersion();

        [DllImport("__Internal")]
        private static extern void acbridge_getPricePoints();

        [DllImport("__Internal")]
        private static extern void acbridge_openSubscriptionManager(string url);

        [DllImport("__Internal")]
        private static extern void acbridge_setPortraitOrientationLock(bool portraitOrientationLock);

        [DllImport("__Internal")]
        private static extern void acbridge_setDebugModeEnabled(bool debugModeEnabled);

        [DllImport("__Internal")]
        private static extern void acbridge_setBrowserMode(string mode);
#else
        // Stub implementations when not on iOS
        private static void acbridge_initialize(string configJson, string customerId, string platformIntegrationVersion = null) { }
        private static void acbridge_openCheckout(string sessionToken, string purchaseId, string url) { }
        private static void acbridge_handleDeepLink(string url) { }
        private static string acbridge_getSdkVersion() { return UNITY_SDK_VERSION; }
        private static void acbridge_getPricePoints() { }
        private static void acbridge_openSubscriptionManager(string url) { }
        private static void acbridge_setPortraitOrientationLock(bool portraitOrientationLock) { }
        private static void acbridge_setDebugModeEnabled(bool debugModeEnabled) { }
        private static void acbridge_setBrowserMode(string mode) { }
#endif
        private bool _eventHandlerInitialized = false;
        private iOSBrowserMode _browserMode = iOSBrowserMode.SFSVC;
        private bool _portraitOrientationLock = false;
        private bool _debugMode = false;
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
            AppchargeConfig editorConfig = ConfigUtility.GetConfig();
            
            if (editorConfig == null) {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }
            
            _browserMode = editorConfig.iOSBrowserMode;
            _debugMode = editorConfig.EnableDebugMode;
            _portraitOrientationLock = editorConfig.PortraitOrientationLock;    
            _applinksDomain = editorConfig.AssociatedDomain;

            InternalInit(checkoutToken, environment, customerId, callback, _applinksDomain, _browserMode == iOSBrowserMode.SFSVC);   
            SetPortraitOrientationLock(_portraitOrientationLock);
        }

        private void InternalInit(string checkoutPublicKey, string environment, string customerId, ICheckoutPurchase callback, string applinksDomain, bool useSFSVC)
        {
            _customerId = customerId;
            Callback = callback;
            InitEventHandler(callback);

            string redirectUrl = null;
            if (!useSFSVC && !string.IsNullOrEmpty(applinksDomain))
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

            acbridge_initialize(configModel.ToJson(), customerId, "Unity " + Application.unityVersion + ", Unity SDK " + UNITY_SDK_VERSION); 
        }

        private void InitEventHandler(ICheckoutPurchase callback) 
        {
            if (_eventHandlerInitialized && _nativeCallbackHandler != null && _nativeCallbackHandler.gameObject != null)
            {
                _nativeCallbackHandler.Inject(callback, this);
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
                _nativeCallbackHandler.Inject(callback, this);
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
                                   
        public void GetPricePoints() {
            acbridge_getPricePoints();
        }

        public void OpenSubscriptionManager(string url) {
            acbridge_openSubscriptionManager(url);
        }

        public void ConfigurePlatform(string property, object value) {
           if (property.Equals("browserMode") && value is iOSBrowserMode)
           {
               SetBrowserMode((iOSBrowserMode)value);
           }

           if (property.Equals("portraitOrientationLock") && value is bool)
           {
               SetPortraitOrientationLock((bool)value);
           }

           if (property.Equals("debugMode") && value is bool)
           {
               SetDebugModeEnabled((bool)value);
           }
        }

        private void SetPortraitOrientationLock(bool portraitOrientationLock) {
            _portraitOrientationLock = portraitOrientationLock;
            acbridge_setPortraitOrientationLock(portraitOrientationLock);
        }

        private void SetBrowserMode(iOSBrowserMode mode) {
            _browserMode = mode;
            acbridge_setBrowserMode(mode.ToString());
        }            

        private void SetDebugModeEnabled(bool debugModeEnabled) {
            _debugMode = debugModeEnabled;
            acbridge_setDebugModeEnabled(debugModeEnabled);
        }

        public void OnInitialized() {
            SetDebugModeEnabled(_debugMode);
            SetBrowserMode(_browserMode);
            SetPortraitOrientationLock(_portraitOrientationLock);
        }
    }
}