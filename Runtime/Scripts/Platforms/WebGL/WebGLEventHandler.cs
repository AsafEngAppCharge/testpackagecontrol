
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
            _callbacks.OnInitialized();
        }

        public void OnInitializeFailed(string errorCode) {
            int code;
            int.TryParse(errorCode, out code);
            
            ErrorMessage errorMessage = new ErrorMessage
            {
                code = code,
                message = "OnInitializeFailed"
            };
            _callbacks.OnInitializeFailed(errorMessage);
        }
        
        public void OnPurchaseSuccess(string eventData)
        {
            try
            {
                OrderResponseModel orderResponseModel = JsonUtility.FromJson<OrderResponseModel>(eventData);
                _callbacks.OnPurchaseSuccess(orderResponseModel);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deserializing 'data' into OrderResponseModel: {ex.Message}");
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
            _callbacks.OnPurchaseFailed(purchaseFailError);
        }

        public void OnPricePointsSuccess(string eventData) {
            try {
                var pricePointsModel = JsonUtility.FromJson<PricePointsModel>(eventData);
                _callbacks.OnPricePointsSuccess(pricePointsModel);
            }
            catch (Exception ex) {
                Debug.LogError($"Error deserializing 'data' into PricePointsModel: {ex.Message}");
            }
        }

        public void OnPricePointsFail(string errorCode) {
            int code;
            int.TryParse(errorCode, out code);
            
            ErrorMessage errorMessage = new ErrorMessage {
                code = code,
                message = "OnPricePointsFail"
            };
            _callbacks.OnPricePointsFail(errorMessage);
        }
    }
}