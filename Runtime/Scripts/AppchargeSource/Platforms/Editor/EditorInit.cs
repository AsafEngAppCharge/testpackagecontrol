#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using Appcharge.PaymentLinks.Platforms.Base;
using Appcharge.PaymentLinks.Platforms.Editor.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorInit : BaseInit
    {
        private EditorPlatform _editorPlatform;
        
        public EditorInit(ICheckoutPlatform platform, EditorPlatform editorPlatform) : base(platform)
        {
            _editorPlatform = editorPlatform;
        }

        public override void Initialize(string customerId, ICheckoutPurchase callback)
        {
            var config = ConfigUtility.GetConfig();
            if (config == null) {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }

            Debug.Log("Checkout Public Key: " + config.CheckoutPublicKey);
            Debug.Log("Environment: " + config.Environment.ToString().ToLowerInvariant());
            Debug.Log("Customer ID: " + customerId);
            Debug.Log("Callback: " + callback);
            Initialize(config.CheckoutPublicKey, config.Environment.ToString().ToLowerInvariant(), customerId, callback);
        }
        
        public override void Initialize(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback)
        {
            _editorPlatform.CheckoutPublicKey = checkoutToken;
            _editorPlatform.Environment = environment;
            EditorPlatform.SharedCoroutineRunner.StartCoroutine(InitializeCoroutine(checkoutToken, environment, customerId, callback));
        }
        
        private IEnumerator InitializeCoroutine(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback)
        {
            var baseUrl = GetBaseUrl(environment);
            var url = $"{baseUrl}/mobile/v2/boot";
            
            var queryParams = new Dictionary<string, string>
            {
                {"ip", "127.0.0.1"},
                {"environment", environment},
                {"checkoutPublicKey", checkoutToken},
                {"platform", "Editor"},
                {"language", "en_US"},
                {"screen", "Resolution: 1920x1080, dpi: 96"},
                {"deviceModel", "Unity Editor"},
                {"deviceManufacturer", "Unity Technologies"},
                {"packageName", Application.identifier},
                {"sdkV", "2.1.0"},
                {"sdkType", "Unity-Editor"},
                {"deviceId", SystemInfo.deviceUniqueIdentifier},
                {"customerId", customerId},
                {"gaid", "Editor-GAID-12345"}
            };
            
            var fullUrl = $"{url}?{BuildQueryString(queryParams)}";
            
            using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
            {
                request.SetRequestHeader("X-Checkout-Token", checkoutToken);
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    _editorPlatform.BootData = JsonUtility.FromJson<EditorBootResponse>(request.downloadHandler.text);
                    _editorPlatform.CustomerId = customerId;
                    callback.OnInitialized();
                }
                else
                {
                    var errorMessage = new ErrorMessage { message = request.error, code = 0 };
                    callback.OnInitializeFailed(errorMessage);
                }
            }
        }
        
        private string GetBaseUrl(string environment)
        {
            return environment.ToLower() switch
            {
                "development" => "https://api-dev.appcharge.com",
                "staging" => "https://api-staging.appcharge.com",
                "sandbox" => "https://api-sandbox.appcharge.com",
                "production" => "https://api.appcharge.com",
                _ => "https://api-sandbox.appcharge.com"
            };
        }
        
        private string BuildQueryString(Dictionary<string, string> parameters)
        {
            var queryParts = new List<string>();
            foreach (var param in parameters)
            {
                queryParts.Add($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}");
            }
            return string.Join("&", queryParts);
        }
    }
}
#endif
