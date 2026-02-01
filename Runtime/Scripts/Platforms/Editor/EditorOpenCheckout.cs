#if UNITY_EDITOR
using System.Collections;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Platforms.Base;
using Appcharge.PaymentLinks.Platforms.Editor.Models;
using Appcharge.PaymentLinks.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorOpenCheckout : BaseOpenCheckout
    {
        private EditorPlatform _editorPlatform;
        
        public EditorOpenCheckout(ICheckoutPlatform platform, EditorPlatform editorPlatform) : base(platform)
        {
            _editorPlatform = editorPlatform;
        }

        /// <summary>
        /// Opens checkout using the url and session token.
        /// </summary>
        /// <param name="url">The url to open the checkout</param>
        /// <param name="sessionToken">The session token to open the checkout</param>
        /// <param name="purchaseId">The purchase id to open the checkout</param>
        public override void OpenCheckout(string url, string sessionToken, string purchaseId)
        {
            string checkoutUrl = url + "/" + sessionToken + "?cot=" + _editorPlatform.CheckoutPublicKey;
            
            Application.OpenURL(checkoutUrl);
            if (_editorPlatform != null)
            {
                var existingRunners = Object.FindObjectsOfType<OrderValidationRunner>();
                foreach (var runner in existingRunners)
                {
                    runner.StopValidation();
                    Object.DestroyImmediate(runner.gameObject);
                }
                
                EditorLoaderManager.Instance.ShowLoader(OnValidationCanceled);
                
                var coroutineRunner = new GameObject("OrderValidationRunner");
                var validationRunner = coroutineRunner.AddComponent<OrderValidationRunner>();
                validationRunner.StartValidation(_editorPlatform, sessionToken, purchaseId);
            }
        }

        /// <summary>
        /// Opens checkout using the parsedUrl from the session creation response.
        /// This method extracts the session token from parsedUrl and adds SDK parameters.
        /// </summary>
        /// <param name="purchaseId">The purchase ID from the session creation response</param>
        /// <param name="parsedUrl">The parsedUrl from the session creation response</param>
        public override void OpenCheckout(string purchaseId, string parsedUrl)
        {
            if (string.IsNullOrEmpty(parsedUrl))
            {
                Debug.LogError("parsedUrl cannot be null or empty");
                return;
            }

            // Extract session token from parsedUrl
            // The session token is between the last '/' and the '#boot' fragment
            string sessionToken = ExtractSessionTokenFromParsedUrl(parsedUrl);
            
            if (string.IsNullOrEmpty(sessionToken))
            {
                Debug.LogError("Failed to extract session token from parsedUrl");
                return;
            }

            // Construct the final URL by adding SDK parameters to parsedUrl
            string checkoutUrl = AddSdkParametersToParsedUrl(parsedUrl);
            
            Application.OpenURL(checkoutUrl);
            
            if (_editorPlatform != null)
            {
                var existingRunners = Object.FindObjectsOfType<OrderValidationRunner>();
                foreach (var runner in existingRunners)
                {
                    runner.StopValidation();
                    Object.DestroyImmediate(runner.gameObject);
                }
                
                EditorLoaderManager.Instance.ShowLoader(OnValidationCanceled);
                
                var coroutineRunner = new GameObject("OrderValidationRunner");
                var validationRunner = coroutineRunner.AddComponent<OrderValidationRunner>();
                validationRunner.StartValidation(_editorPlatform, sessionToken, purchaseId);
            }
        }

        /// <summary>
        /// Extracts the session token from the parsedUrl.
        /// The session token is located between the last '/' and the '#boot' fragment.
        /// </summary>
        /// <param name="parsedUrl">The parsedUrl containing the session token</param>
        /// <returns>The extracted session token, or null if extraction fails</returns>
        private string ExtractSessionTokenFromParsedUrl(string parsedUrl)
        {
            // Find the position of the #boot fragment
            int bootFragmentIndex = parsedUrl.IndexOf("#boot");
            if (bootFragmentIndex == -1)
            {
                // If no #boot fragment, try to extract from the last path segment
                int lastSlashIndex = parsedUrl.LastIndexOf('/');
                if (lastSlashIndex == -1 || lastSlashIndex == parsedUrl.Length - 1)
                {
                    return null;
                }
                
                // Check if there's a query string or fragment after the last slash
                int queryOrFragmentIndex = parsedUrl.IndexOfAny(new char[] { '?', '#' }, lastSlashIndex + 1);
                if (queryOrFragmentIndex == -1)
                {
                    return parsedUrl.Substring(lastSlashIndex + 1);
                }
                else
                {
                    return parsedUrl.Substring(lastSlashIndex + 1, queryOrFragmentIndex - lastSlashIndex - 1);
                }
            }

            // Find the last '/' before the #boot fragment
            int lastSlashBeforeBoot = parsedUrl.LastIndexOf('/', bootFragmentIndex);
            if (lastSlashBeforeBoot == -1 || lastSlashBeforeBoot >= bootFragmentIndex - 1)
            {
                return null;
            }

            // Extract the session token between the last '/' and '#boot'
            return parsedUrl.Substring(lastSlashBeforeBoot + 1, bootFragmentIndex - lastSlashBeforeBoot - 1);
        }

        /// <summary>
        /// Adds SDK parameters (cot, platform, browserType, redirectUrl) to the parsedUrl.
        /// The parameters are inserted before the #boot fragment if present, or appended to the URL.
        /// </summary>
        /// <param name="parsedUrl">The parsedUrl to add parameters to</param>
        /// <returns>The URL with SDK parameters added</returns>
        private string AddSdkParametersToParsedUrl(string parsedUrl)
        {
            // Build the query parameters
            string queryParams = $"cot={UnityWebRequest.EscapeURL(_editorPlatform.CheckoutPublicKey)}&platform=editor&browserType=editor&redirectUrl={UnityWebRequest.EscapeURL("acnative://action")}";

            // Check if parsedUrl already has query parameters
            int queryIndex = parsedUrl.IndexOf('?');
            int fragmentIndex = parsedUrl.IndexOf('#');
            
            if (queryIndex != -1)
            {
                // If there are existing query parameters, append to them
                if (fragmentIndex != -1 && fragmentIndex > queryIndex)
                {
                    // Query parameters exist and fragment comes after - append to query string before fragment
                    return parsedUrl.Substring(0, fragmentIndex) + "&" + queryParams + parsedUrl.Substring(fragmentIndex);
                }
                else
                {
                    // Query parameters exist but no fragment, or fragment is before query (unlikely)
                    return parsedUrl + "&" + queryParams;
                }
            }
            else if (fragmentIndex != -1)
            {
                // No query parameters, but fragment exists - insert query before fragment
                return parsedUrl.Substring(0, fragmentIndex) + "?" + queryParams + parsedUrl.Substring(fragmentIndex);
            }
            else
            {
                // No query parameters and no fragment - append query
                return parsedUrl + "?" + queryParams;
            }
        }

        private void OnValidationCanceled()
        {
            var runners = Object.FindObjectsOfType<OrderValidationRunner>();
            foreach (var runner in runners)
            {
                runner.StopValidation();
                Object.DestroyImmediate(runner.gameObject);
            }
            
            if (_editorPlatform?.Callback != null)
            {
                _editorPlatform.Callback.OnPurchaseFailed(new ErrorMessage { message = "Order validation canceled by user." });
            }
        }
    }

    public class OrderValidationRunner : MonoBehaviour
    {
        private EditorPlatform _editorPlatform;
        private string _checkoutSessionToken;
        private string _purchaseId;
        private string _validateUrl;
        private float _startTime;
        private float _lastCheckTime;
        private float _checkInterval = 1f;
        private float _timeout = 600f;
        private bool _isValidating = false;
        private bool _originalRunInBackground;

        public void StartValidation(EditorPlatform editorPlatform, string checkoutSessionToken, string purchaseId)
        {
            if (editorPlatform?.BootData == null || string.IsNullOrEmpty(editorPlatform.CustomerId))
            {
                Debug.LogError("Order validation failed: Platform not properly initialized");
                return;
            }

            StopValidation();

            _editorPlatform = editorPlatform;
            _checkoutSessionToken = checkoutSessionToken;
            _purchaseId = purchaseId;
            _validateUrl = $"{editorPlatform.BootData.appchargeUrl}{editorPlatform.BootData.getOrderPath}/{purchaseId}/player/{editorPlatform.CustomerId}";
            _startTime = (float)EditorApplication.timeSinceStartup;
            _lastCheckTime = 0f;
            _isValidating = true;

            _originalRunInBackground = EditorApplication.isPaused;
            if (_originalRunInBackground)
            {
                EditorApplication.isPaused = false;
            }

            EditorApplication.update += ValidateOrderUpdate;
        }

        private void ValidateOrderUpdate()
        {
            if (!_isValidating)
            {
                EditorApplication.update -= ValidateOrderUpdate;
                return;
            }

            var currentTime = (float)EditorApplication.timeSinceStartup;
            var elapsed = currentTime - _startTime;
            
            if (elapsed > _timeout)
            {
                _editorPlatform.Callback.OnPurchaseFailed(new ErrorMessage { message = "Order validation timed out."});
                FocusUnityEditor();
                StopValidation();
                DestroyImmediate(gameObject);
                return;
            }

            if (currentTime - _lastCheckTime >= _checkInterval)
            {
                _lastCheckTime = currentTime;
                StartCoroutine(ValidateOrderRequest());
            }
        }

        private IEnumerator ValidateOrderRequest()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(_validateUrl))
            {
                request.SetRequestHeader("X-Checkout-Token", _editorPlatform.CheckoutPublicKey);
                request.SetRequestHeader("Authorization", $"Bearer {_checkoutSessionToken}");

                yield return new WaitForSeconds(2f);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var apiResponse = JsonUtility.FromJson<OrderValidationApiResponse>(request.downloadHandler.text);

                        if (apiResponse.state == "charge_succeed")
                        {
                            Debug.Log("response: " + request.downloadHandler.text);
                            
                            var orderResponse = ConvertToOrderResponseModel(apiResponse);
                            _editorPlatform.Callback.OnPurchaseSuccess(orderResponse);
                            
                            FocusUnityEditor();
                            StopValidation();
                            DestroyImmediate(gameObject);
                        }
                        else if (apiResponse.state == "charge_failed")
                        {
                            var orderResponse = ConvertToOrderResponseModel(apiResponse);
                            _editorPlatform.Callback.OnPurchaseFailed(new ErrorMessage { message = apiResponse.reason });
                            FocusUnityEditor();
                            StopValidation();
                            DestroyImmediate(gameObject);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void StopValidation()
        {
            if (_isValidating)
            {
                _isValidating = false;
                EditorApplication.update -= ValidateOrderUpdate;
                EditorApplication.isPaused = _originalRunInBackground;
                EditorLoaderManager.Instance.HideLoader();
            }
        }
        
        private void OnDestroy()
        {
            StopValidation();
            EditorLoaderManager.Cleanup();
        }

        private OrderResponseModel ConvertToOrderResponseModel(OrderValidationApiResponse apiResponse)
        {
            var orderResponse = new OrderResponseModel
            {
                currency = apiResponse.totalSumCurrency,
                sessionToken = apiResponse.sessionId,
                customerId = apiResponse.userId,
                purchaseId = _purchaseId,
                paymentMethodName = apiResponse.paymentMethodName,
                offerSku = apiResponse.bundleSKU,
                price = apiResponse.totalSum,
                offerName = apiResponse.bundleName,
                date = apiResponse.date,
                customerCountry = apiResponse.userCountry,
                orderId = apiResponse.orderId
            };

            if (apiResponse.products != null && apiResponse.products.Length > 0)
            {
                orderResponse.items = new ProductModel[apiResponse.products.Length];
                for (int i = 0; i < apiResponse.products.Length; i++)
                {
                    orderResponse.items[i] = new ProductModel
                    {
                        name = apiResponse.products[i].name,
                        sku = apiResponse.products[i].sku,
                        amount = apiResponse.products[i].amount
                    };
                }
            }

            return orderResponse;
        }


        private void FocusUnityEditor()
        {
            TryFocusWindow("Window/General/Console");
            TryFocusWindow("Window/General/Hierarchy");
            TryFocusWindow("Window/General/Project");
            
            bool gameFocused = TryFocusWindow("Window/General/Game");
            
            if (!gameFocused)
            {
                TryFocusWindow("Window/General/Simulator");
            }
            
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();
            
            Debug.Log("Order validation completed - Unity Editor focused");
        }
        
        private bool TryFocusWindow(string menuPath)
        {
            try
            {
                EditorApplication.ExecuteMenuItem(menuPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
#endif