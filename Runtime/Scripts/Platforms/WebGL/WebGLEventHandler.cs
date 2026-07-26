
using System;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.WebGL {
    public class WebGLEventHandler : MonoBehaviour
    {
        private ICheckoutPurchase _callbacks;

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

        public void OnInitialized() {
            Debug.Log($"[WebGLEventHandler] OnInitialized go={gameObject.GetInstanceID()} callbacks={(_callbacks == null ? "NULL" : _callbacks.GetType().Name)}");
            _callbacks?.OnInitialized();
        }

        public void OnInitializeFailed(string errorCode) {
            int code;
            int.TryParse(errorCode, out code);
            
            ErrorMessage errorMessage = new ErrorMessage
            {
                code = code,
                message = "OnInitializeFailed"
            };
            _callbacks?.OnInitializeFailed(errorMessage);
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
                _callbacks.OnPurchaseSuccess(orderResponseModel);
                Debug.Log("OnPurchaseSuccess: Successfully deserialized order JSON into OrderResponseModel: " + orderResponseModel.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deserializing order JSON into OrderResponseModel: {ex.Message}");
            }
        }

        public void OnPurchaseFailed(string errorCode) {
            int code;
            int.TryParse(errorCode, out code);
            
            ErrorMessage purchaseFailError = new ErrorMessage
            {
                code = code,
                message = "OnPurchaseFailed"
            };
            _callbacks.OnPurchaseFailed(purchaseFailError, null);
        }
    }
}