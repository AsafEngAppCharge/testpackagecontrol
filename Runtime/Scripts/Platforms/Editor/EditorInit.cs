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
        private static bool _initInProgress;

        private EditorPlatform _editorPlatform;
        
        public EditorInit(ICheckoutPlatform platform, EditorPlatform editorPlatform) : base(platform)
        {
            _editorPlatform = editorPlatform;
        }

        public override void Initialize(ICheckoutPurchase callback)
        {
            var config = ConfigUtility.GetConfig();
            if (config == null) {
                Debug.LogError("AppchargeConfig not found.");
                return;
            }

            Initialize(config.CheckoutPublicKey, config.Environment.ToString().ToLowerInvariant(), callback);
        }
        
        public override void Initialize(string checkoutToken, string environment, ICheckoutPurchase callback)
        {
            if (_initInProgress)
            {
                callback.OnInitializeFailed(new ErrorMessage
                {
                    code = EditorErrorCodes.BootInitAlreadyInProgress,
                    message = EditorErrorCodes.BootInitAlreadyInProgressMessage
                });
                return;
            }

            _initInProgress = true;
            _editorPlatform.CheckoutPublicKey = checkoutToken;
            _editorPlatform.Environment = environment;
            EditorPlatform.SharedCoroutineRunner.StartCoroutine(InitializeCoroutine(checkoutToken, environment, callback));
        }
        
        private IEnumerator InitializeCoroutine(string checkoutToken, string environment, ICheckoutPurchase callback)
        {
            try
            {
                var baseUrl = GetBaseUrl(environment);
                var url = $"{baseUrl}/mobile/v4/boot";
                
                var queryParams = new Dictionary<string, string>
                {
                    {"apiLevel", "2"},
                };
                
                var fullUrl = $"{url}?{BuildQueryString(queryParams)}";
                
                using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
                {
                    request.SetRequestHeader("X-Checkout-Token", checkoutToken);
                    yield return request.SendWebRequest();
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        _editorPlatform.BootData = JsonUtility.FromJson<EditorBootResponse>(request.downloadHandler.text);
                        _editorPlatform.OrderValidationTimeout = _editorPlatform.BootData.orderValidationTimeout * 1000;
                        Debug.Log("Initialization success");
                        callback.OnInitialized();
                    }
                    else
                    {
                        callback.OnInitializeFailed(new ErrorMessage { message = request.error, code = EditorErrorCodes.BootInitializationError });
                    }
                }
            }
            finally
            {
                _initInProgress = false;
            }
        }
        
        private string GetBaseUrl(string environment)
        {
            return environment.ToLower() switch
            {
                "staging" => "https://ext-stg-api.appchargestore.com",
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
