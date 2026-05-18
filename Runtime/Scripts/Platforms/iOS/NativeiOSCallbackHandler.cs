using System;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.iOS {
    public class NativeiOSCallbackHandler : MonoBehaviour
    {
        private ICheckoutPurchase _checkoutCallback;
        private iOSPlatform _platform;

        public void Inject(ICheckoutPurchase checkoutCallback, iOSPlatform platform)
        {
            _checkoutCallback = checkoutCallback;
            _platform = platform;
        }

        public void OnInitialized()
        {
            _checkoutCallback?.OnInitialized();
            _platform.OnInitialized();
        }

        public void OnInitializeFailed(string errorJson)
        {
            var error = JsonUtility.FromJson<ErrorMessage>(errorJson);
            _checkoutCallback?.OnInitializeFailed(error);
        }

        public void OnPurchaseSuccess(string orderJson)
        {
            var order = JsonUtility.FromJson<OrderResponseModel>(orderJson);
            _checkoutCallback?.OnPurchaseSuccess(order);
        }

        public void OnPurchaseFailed(string payloadJson)
        {
            var payload = JsonUtility.FromJson<PurchaseFailedPayload>(payloadJson);
            var error = JsonUtility.FromJson<ErrorMessage>(payload.errorJson);
            OrderResponseModel order = null;
            if (!string.IsNullOrEmpty(payload.orderJson) && payload.orderJson != "null")
            {
                order = JsonUtility.FromJson<OrderResponseModel>(payload.orderJson);
            }
            _checkoutCallback?.OnPurchaseFailed(error, order);
        }

        public void OnPricePointsSuccess(string pricePointsJson)
        {
            var pricePoints = JsonUtility.FromJson<PricePointsModel>(pricePointsJson);
            _checkoutCallback?.OnPricePointsSuccess(pricePoints);
        }

        public void OnPricePointsFail(string errorJson)
        {
            var error = JsonUtility.FromJson<ErrorMessage>(errorJson);
            _checkoutCallback?.OnPricePointsFail(error);
        }
    }

    [Serializable]
    internal class PurchaseFailedPayload
    {
        public string errorJson;
        public string orderJson;
    }
}