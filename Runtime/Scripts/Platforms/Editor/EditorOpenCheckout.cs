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

        public override void OpenCheckout(string purchaseId, string parsedUrl, string customerId)
        {
            var callback = _editorPlatform?.Callback;

            if (string.IsNullOrWhiteSpace(customerId))
            {
                FailOpenCheckout(callback, EditorErrorCodes.InvalidArgumentCustomerId, EditorErrorCodes.InvalidArgumentCustomerIdMessage);
                return;
            }

            if (string.IsNullOrWhiteSpace(purchaseId))
            {
                FailOpenCheckout(callback, EditorErrorCodes.InvalidArgumentPurchaseId, string.Format(EditorErrorCodes.InvalidArgumentPurchaseIdMessage, purchaseId ?? string.Empty));
                return;
            }

            if (string.IsNullOrWhiteSpace(parsedUrl))
            {
                FailOpenCheckout(callback, EditorErrorCodes.InvalidArgumentParsedUrl, EditorErrorCodes.InvalidArgumentParsedUrlMessage);
                return;
            }

            string sessionToken = ExtractSessionTokenFromParsedUrl(parsedUrl);
            if (string.IsNullOrEmpty(sessionToken))
            {
                FailOpenCheckout(callback, EditorErrorCodes.InvalidArgumentParsedUrl, EditorErrorCodes.InvalidArgumentParsedUrlMessage);
                return;
            }

            string checkoutUrl = AddSdkParametersToParsedUrl(parsedUrl);
            
            Application.OpenURL(checkoutUrl);
            
            if (_editorPlatform != null)
            {
                _editorPlatform.CustomerId = customerId;
                var existingRunners = Object.FindObjectsOfType<OrderValidationRunner>();
                foreach (var runner in existingRunners)
                {
                    runner.StopAndResetValidation();
                    Object.DestroyImmediate(runner.gameObject);
                }
                
                EditorLoaderManager.Instance.ShowLoader(OnValidationCanceled);
                
                var coroutineRunner = new GameObject("OrderValidationRunner");
                var validationRunner = coroutineRunner.AddComponent<OrderValidationRunner>();
                validationRunner.StartValidation(_editorPlatform, sessionToken, purchaseId);
            }
        }

        private static void FailOpenCheckout(ICheckoutPurchase callback, int code, string message)
        {
            Debug.LogWarning(message);
            callback?.OnPurchaseFailed(new ErrorMessage { code = code, message = message }, null);
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
        /// Adds checkout and origin query parameters to the parsedUrl.
        /// Parameters are inserted before the #boot fragment if present, or appended to the URL.
        /// </summary>
        private string AddSdkParametersToParsedUrl(string parsedUrl)
        {
            return AppendQueryParamsToUrl(parsedUrl, BuildCheckoutQueryParams());
        }

        private string BuildCheckoutQueryParams()
        {
            string sourceVersion = UnityWebRequest.EscapeURL(_platform.GetSdkVersion());
            return $"cot={UnityWebRequest.EscapeURL(_editorPlatform.CheckoutPublicKey)}" +
                   "&platform=editor" +
                   "&browserType=editor" +
                   $"&redirectUrl={UnityWebRequest.EscapeURL("acnative://action")}" +
                   "&resource=pl_sdk" +
                   $"&source_version={sourceVersion}" +
                   $"&engine=unity";
        }

        private static string AppendQueryParamsToUrl(string url, string queryParams)
        {
            int queryIndex = url.IndexOf('?');
            int fragmentIndex = url.IndexOf('#');

            if (queryIndex != -1)
            {
                if (fragmentIndex != -1 && fragmentIndex > queryIndex)
                {
                    return url.Substring(0, fragmentIndex) + "&" + queryParams + url.Substring(fragmentIndex);
                }

                return url + "&" + queryParams;
            }

            if (fragmentIndex != -1)
            {
                return url.Substring(0, fragmentIndex) + "?" + queryParams + url.Substring(fragmentIndex);
            }

            return url + "?" + queryParams;
        }

        private void OnValidationCanceled()
        {
            var runners = Object.FindObjectsOfType<OrderValidationRunner>();
            foreach (var runner in runners)
            {
                runner.FinalizeUserCanceled(_editorPlatform?.Callback);
            }
        }
    }

    public class OrderValidationRunner : MonoBehaviour
    {
        private EditorPlatform _editorPlatform;
        private string _checkoutSessionToken;
        private string _purchaseId;
        private string _validateUrl;
        private bool _isValidating;
        private bool _originalRunInBackground;
        private bool _requestInFlight;
        private float _checkInterval;
        private float _lastCheckTime;
        private float _validationTimeoutSeconds;
        private float _chargePendingStartTime;

        public void StartValidation(EditorPlatform editorPlatform, string checkoutSessionToken, string purchaseId)
        {
            if (editorPlatform?.BootData == null || string.IsNullOrEmpty(editorPlatform.CustomerId))
            {
                Debug.LogError("Order validation failed: Platform not properly initialized");
                return;
            }

            StopAndResetValidation();

            _editorPlatform = editorPlatform;
            _checkoutSessionToken = checkoutSessionToken;
            _purchaseId = purchaseId;
            _validateUrl = $"{editorPlatform.BootData.appchargeUrl}{editorPlatform.BootData.getOrderPath}/{purchaseId}/player/{editorPlatform.CustomerId}";
            _checkInterval = editorPlatform.BootData.orderValidationRate / 1000f;
            _validationTimeoutSeconds = editorPlatform.OrderValidationTimeout > 0
                ? editorPlatform.OrderValidationTimeout / 1000f
                : editorPlatform.BootData.orderValidationTimeout / 1000f;
            _lastCheckTime = 0f;
            _chargePendingStartTime = 0f;
            _isValidating = true;

            _originalRunInBackground = EditorApplication.isPaused;
            if (_originalRunInBackground) EditorApplication.isPaused = false;

            EditorApplication.update += ValidateOrderUpdate;
        }

        private void ValidateOrderUpdate()
        {
            if (!_isValidating)
            {
                EditorApplication.update -= ValidateOrderUpdate;
                return;
            }

            if (_requestInFlight) 
                return;

            var currentTime = (float)EditorApplication.timeSinceStartup;

            if (_lastCheckTime > 0f && currentTime - _lastCheckTime < _checkInterval) return;

            _requestInFlight = true;
            StartCoroutine(ValidateOrderRequest());
        }

        private IEnumerator ValidateOrderRequest()
        {
            yield return ValidateOrderOnce();

            _requestInFlight = false;
            _lastCheckTime = (float)EditorApplication.timeSinceStartup;

            if (!_isValidating) yield break;

            if (ProcessOrderValidationResponse(_editorPlatform.Callback))
                CleanValidation();
        }

        public void FinalizeUserCanceled(ICheckoutPurchase callback)
        {
            if (callback == null || !_isValidating) return;

            StopAndResetValidation();
            StartCoroutine(FinalizeUserCanceledRoutine(callback));
        }

        private IEnumerator FinalizeUserCanceledRoutine(ICheckoutPurchase callback)
        {
            yield return ValidateOrderOnce();

            if (ProcessOrderValidationResponse(callback))
            {
                FinishFinalize();
                yield break;
            }

            OnCanceledPayment(callback);
            FinishFinalize();
        }

        private IEnumerator ValidateOrderOnce()
        {
            using (var request = UnityWebRequest.Get(_validateUrl))
            {
                request.SetRequestHeader("X-Checkout-Token", _editorPlatform.CheckoutPublicKey);
                request.SetRequestHeader("Authorization", $"Bearer {_checkoutSessionToken}");
                yield return request.SendWebRequest();

                _validationSuccess = request.result == UnityWebRequest.Result.Success;
                _validationHttpCode = request.responseCode;
                _pollResponse = null;
                if (_validationSuccess)
                {
                    try { _pollResponse = JsonUtility.FromJson<OrderValidationApiResponse>(request.downloadHandler.text); }
                    catch { _validationSuccess = false; }
                }
            }
        }

        private bool _validationSuccess;
        private long _validationHttpCode;
        private OrderValidationApiResponse _pollResponse;

        private bool ProcessOrderValidationResponse(ICheckoutPurchase callback)
        {
            if (_validationHttpCode == 404 || _pollResponse?.state == "order not found")
            {
                return false;
            }

            if (!_validationSuccess) return false;

            var state = _pollResponse.state?.ToLowerInvariant();
            var orderResponse = ConvertToOrderResponseModel(_pollResponse);

            if (state == "created")
            {
                return false;
            }

            if (state == "payment_canceled")
            {
                callback.OnPurchaseCanceled(
                    new ErrorMessage { code = EditorErrorCodes.ChargeCanceled, message = EditorErrorCodes.OrderValidationCanceledMessage }, orderResponse);
                return true;
            }

            if (state == "charge_success" || state == "charge_succeed")
            {
                callback.OnPurchaseSuccess(orderResponse);
                return true;
            }

            if (state == "charge_failed")
            {
                callback.OnPurchaseFailed(
                    new ErrorMessage { code = EditorErrorCodes.ChargeFailed, message = _pollResponse.reason ?? EditorErrorCodes.ChargeFailedName }, orderResponse);
                return true;
            }

            if (state == "payment_pending") {
                return false;
            }

            if (state == "charge_pending")
            {
                var elapsedTime = (float)EditorApplication.timeSinceStartup;
                if (_chargePendingStartTime <= 0f)
                    _chargePendingStartTime = elapsedTime;

                if (elapsedTime - _chargePendingStartTime < _validationTimeoutSeconds)
                    return false;

                callback.OnPurchaseFailed(
                    new ErrorMessage { code = EditorErrorCodes.ValidateOrderTimeout, message = EditorErrorCodes.ValidateOrderTimeoutMessage }, orderResponse);
                return true;
            }

            return false;
        }

        private void FinishFinalize()
        {
            FocusUnityEditor();
            StopAllCoroutines();
            DestroyImmediate(gameObject);
        }

        public void OnCanceledPayment(ICheckoutPurchase callback)
        {
            if (callback == null) return;

            var orderResponse = ConvertToOrderResponseModel(_pollResponse);
            callback.OnPurchaseCanceled(
                new ErrorMessage { code = EditorErrorCodes.ChargeCanceled, message = EditorErrorCodes.OrderValidationCanceledMessage }, orderResponse);
        }

        private void CleanValidation()
        {
            FocusUnityEditor();
            StopAndResetValidation();
            StopAllCoroutines();
            DestroyImmediate(gameObject);
        }

        public void StopAndResetValidation()
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
            StopAndResetValidation();
            EditorLoaderManager.Cleanup();
        }

        private OrderResponseModel ConvertToOrderResponseModel(OrderValidationApiResponse apiResponse)
        {
            if (apiResponse == null || string.IsNullOrEmpty(apiResponse.orderId)) return null;

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
            if (!TryFocusWindow("Window/General/Game")) TryFocusWindow("Window/General/Simulator");
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();
        }

        private static bool TryFocusWindow(string menuPath)
        {
            try { EditorApplication.ExecuteMenuItem(menuPath); return true; }
            catch { return false; }
        }
    }
}
#endif