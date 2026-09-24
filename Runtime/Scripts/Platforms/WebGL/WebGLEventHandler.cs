
using System;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.WebGL {
    public class WebGLEventHandler : MonoBehaviour
    {
        private ICheckoutPurchase _callbacks;
        private WebGLCheckoutData _checkoutData = new WebGLCheckoutData();

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void Inject(ICheckoutPurchase callbacks)
        {
            if (callbacks == null)
            {
                Debug.LogError("Callbacks are null in Inject method.");
                return;
            }

            _callbacks = callbacks;
        }

        public void SetCheckoutData(string purchaseId, string parsedUrl, string customerId)
        {
            _checkoutData.Set(purchaseId, parsedUrl, customerId);
        }

        public void OnInitialized() {
            _callbacks?.OnInitialized();
        }

        public void OnInitializeFailed(string json) {
            var error = JsonUtility.FromJson<ErrorMessage>(json);
            _callbacks?.OnInitializeFailed(error);
        }
        
        public void OnPurchaseSuccess(string eventData)
        {
            if (string.IsNullOrEmpty(eventData))
            {
                Debug.LogError("OnPurchaseSuccess: WebGL bridge sent null or empty order JSON.");
                return;
            }

            try
            {
                OrderResponseModel orderResponseModel = JsonUtility.FromJson<OrderResponseModel>(eventData);
                orderResponseModel = _checkoutData.Enrich(orderResponseModel);
                _callbacks?.OnPurchaseSuccess(orderResponseModel);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deserializing order JSON into OrderResponseModel: {ex.Message}");
            }
        }

        public void OnPurchaseCanceled(string payloadJson)
        {
            DispatchPurchaseResult(payloadJson, (error, order) => _callbacks?.OnPurchaseCanceled(error, order));
        }

        public void OnPurchaseFailed(string payloadJson)
        {
            DispatchPurchaseResult(payloadJson, (error, order) => _callbacks?.OnPurchaseFailed(error, order));
        }

        private void DispatchPurchaseResult(string payloadJson, Action<ErrorMessage, OrderResponseModel> dispatch)
        {
            try
            {
                var (error, order) = PurchaseResultPayloadParser.Parse(payloadJson);
                order = _checkoutData.Enrich(order);
                dispatch(error, order);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error handling WebGL purchase result payload: {ex.Message}");
            }
        }
    }
}
