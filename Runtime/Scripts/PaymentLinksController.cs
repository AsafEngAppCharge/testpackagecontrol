using System;
using UnityEngine;
using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Threading;
using Appcharge.PaymentLinks.Platforms.Unsupported;
using Appcharge.PaymentLinks.Platforms.iOS;
using Appcharge.PaymentLinks.Platforms.Android;
using Appcharge.PaymentLinks.Platforms.WebGL;

namespace Appcharge.PaymentLinks {
    public class PaymentLinksController {
        private static PaymentLinksController _Instance;
        private static ICheckoutPlatform _currentPlatform;

        private static ICheckoutPlatform Platform {
            get {
                if (_currentPlatform != null) {
                    return _currentPlatform;
                }

                switch (Application.platform) {
                    #if UNITY_IOS
                    case RuntimePlatform.IPhonePlayer:
                        _currentPlatform = new iOSPlatform();
                        break;
                    #endif
                    #if UNITY_ANDROID
                    case RuntimePlatform.Android:
                        _currentPlatform = new AndroidPlatform();
                        break;
                    #endif
                    #if UNITY_WEBGL
                    case RuntimePlatform.WebGLPlayer:
                        _currentPlatform = new WebGLPlatform();
                        break;
                    #endif
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.LinuxEditor:
                        _currentPlatform = CreateEditorPlatform();
                        break;
                    default:
                        _currentPlatform = new UnsupportedPlatform();
                        break;
                }

                return _currentPlatform;
            }
        }

        private PaymentLinksController() {
        }

        public static PaymentLinksController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new PaymentLinksController();
                }
                return _Instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void WarmSingletonAfterFirstSceneLoad()
        {
            _ = Instance;
        }

        public void Init(ICheckoutPurchase callback)
        {
            DispatchOnMainThread(InitOnMainThread);

            void InitOnMainThread() {
                ApplyMainThreadDispatcherFromConfig();
                Platform.Init(WrapPublisherCallback(callback));
            }
        }

        public void Init(string checkoutToken, string environment, ICheckoutPurchase callback) {
            DispatchOnMainThread(InitOnMainThread);

            void InitOnMainThread() {
                ApplyMainThreadDispatcherFromConfig();
                Platform.Init(checkoutToken, environment, WrapPublisherCallback(callback));
            }
        }

        public void OpenCheckout(string purchaseId, string parsedUrl, string customerId) {
            DispatchOnMainThread(OpenCheckoutOnMainThread);

            void OpenCheckoutOnMainThread() {
                Platform.OpenCheckout(purchaseId, parsedUrl, customerId);
            }
        }

        public string GetSdkVersion() {
            return DispatchOnMainThread(GetSdkVersionOnMainThread);

            string GetSdkVersionOnMainThread() {
                return Platform.GetSdkVersion();
            }
        }

        public void SetConfiguration(string property, object value) {
            DispatchOnMainThread(SetConfigurationOnMainThread);

            void SetConfigurationOnMainThread() {
                if (property.Equals("enableMainThreadDispatcher", StringComparison.OrdinalIgnoreCase) && value is bool enabled) {
                    MainThreadDispatcher.Enabled = enabled;
                    return;
                }

                Platform.ConfigurePlatform(property, value);
            }
        }

        private static void DispatchOnMainThread(Action action) {
            MainThreadDispatcher.RunSync(action);
        }

        private static T DispatchOnMainThread<T>(Func<T> func) {
            return MainThreadDispatcher.RunSync(func);
        }

        private static ICheckoutPurchase WrapPublisherCallback(ICheckoutPurchase callback) {
            if (callback == null) {
                return null;
            }

            return new MainThreadCheckoutPurchase(callback);
        }

        private static void ApplyMainThreadDispatcherFromConfig() {
            try {
                var config = ConfigUtility.GetConfig();
                MainThreadDispatcher.Enabled = config.EnableMainThreadDispatcher;
                MainThreadDispatcher.DebugLogging = config.EnableDebugMode;
            } catch (Exception) {
                MainThreadDispatcher.Enabled = true;
                MainThreadDispatcher.DebugLogging = false;
            }
        }

        private static ICheckoutPlatform CreateEditorPlatform()
        {
            var editorPlatformType = System.Type.GetType("Appcharge.PaymentLinks.Platforms.Editor.EditorPlatform, Appcharge.PaymentLinks.Platforms.Editor");
            if (editorPlatformType != null)
            {
                try
                {
                    return System.Activator.CreateInstance(editorPlatformType) as ICheckoutPlatform;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to create Editor platform: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Could not find EditorPlatform. Falling back to UnsupportedPlatform.");
            }
            return new UnsupportedPlatform();
        }
    }
}