using UnityEngine;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Platforms.Unsupported;
using Appcharge.PaymentLinks.Platforms.iOS;
using Appcharge.PaymentLinks.Platforms.Android;
using Appcharge.PaymentLinks.Platforms.WebGL;

namespace Appcharge.PaymentLinks {
    public class PaymentLinksController {
        private static PaymentLinksController _Instance;
        private static ICheckoutPlatform _currentPlatform;
        private static bool _definedPlatform = false;
        private PaymentLinksController() { }
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

        public void Awake() {
            DefinePlatform();
        }

        public void DefinePlatform() {
            if (_definedPlatform) {
                return;
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
                    #endif
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                    _currentPlatform = CreateEditorPlatform();
                    break;
                default:
                    if (_currentPlatform == null) {
                        _currentPlatform = new UnsupportedPlatform();
                    }
                    break;
            }

            _definedPlatform = true;
        }

        private ICheckoutPlatform CreatePlatformByName(string assemblyQualifiedName)
        {
            // Try to create the platform-specific instance at runtime
            System.Type platformType = System.Type.GetType(assemblyQualifiedName);
            
            // If Type.GetType fails, search through all loaded assemblies
            if (platformType == null)
            {
                System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (System.Reflection.Assembly assembly in assemblies)
                {
                    try
                    {
                        platformType = assembly.GetType(assemblyQualifiedName.Split(',')[0]);
                        if (platformType != null)
                            break;
                    }
                    catch (System.Exception)
                    {
                        // Continue searching
                    }
                }
            }
            
            if (platformType != null)
            {
                try
                {
                    return System.Activator.CreateInstance(platformType) as ICheckoutPlatform;
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"Failed to create platform {assemblyQualifiedName}: {ex.Message}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Could not find type: {assemblyQualifiedName}. Falling back to UnsupportedPlatform.");
            }
            return new UnsupportedPlatform();
        }

        public void Init(string customerId, ICheckoutPurchase callback)
        {
            DefinePlatform();
            _currentPlatform.Init(customerId, callback);
        }

        public void Init(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback) {
            DefinePlatform();
            _currentPlatform.Init(checkoutToken, environment, customerId, callback);
        }

        public void OpenCheckout(string url, string sessionToken , string purchaseId)        
        {
            _currentPlatform.OpenCheckout(url, sessionToken, purchaseId);
        }

        public string GetSdkVersion() {
            return _currentPlatform.GetSdkVersion();
        }

        public void GetPricePoints() {
            _currentPlatform.GetPricePoints();
        }

        public void OpenSubscriptionManager(string url) {
            _currentPlatform.OpenSubscriptionManager(url);
        }

        public void SetConfiguration(string property, object value) {
            _currentPlatform.ConfigurePlatform(property, value);
        }

        private ICheckoutPlatform CreateEditorPlatform()
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
                    UnityEngine.Debug.LogWarning($"Failed to create Editor platform: {ex.Message}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Could not find EditorPlatform. Falling back to UnsupportedPlatform.");
            }
            return new UnsupportedPlatform();
        }
    }
}