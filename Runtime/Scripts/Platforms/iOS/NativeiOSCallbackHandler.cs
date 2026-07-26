using System;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.iOS {
    public class NativeiOSCallbackHandler : MonoBehaviour
    {
        private iOSPlatform _platform;

        public void Inject(iOSPlatform platform)
        {
            _platform = platform;
        }

        public void OnInitialized(string unused)
        {
            _platform.Callback?.OnInitialized();
            _platform.OnInitialized();
        }

        public void OnInitializeFailed(string json)
        {
            var error = JsonUtility.FromJson<ErrorMessage>(json);
            _platform.Callback?.OnInitializeFailed(error);
        }

        public void OnPurchaseSuccess(string orderJson)
        {
            var order = JsonUtility.FromJson<OrderResponseModel>(orderJson);
            _platform.Callback?.OnPurchaseSuccess(order);
        }

        public void OnPurchaseCanceled(string payloadJson)
        {
            var (error, order) = ParsePurchasePayload(payloadJson);
            _platform.Callback?.OnPurchaseCanceled(error, order);
        }

        public void OnPurchaseFailed(string payloadJson)
        {
            var (error, order) = ParsePurchasePayload(payloadJson);
            _platform.Callback?.OnPurchaseFailed(error, order);
        }

        public void OnPricePointsSuccess(string pricePointsJson)
        {
            var pricePoints = JsonUtility.FromJson<PricePointsModel>(pricePointsJson);
            _platform.Callback?.OnPricePointsSuccess(pricePoints);
        }

        public void OnPricePointsFail(string errorJson)
        {
            var error = JsonUtility.FromJson<ErrorMessage>(errorJson);
            _platform.Callback?.OnPricePointsFail(error);
        }

        private (ErrorMessage error, OrderResponseModel order) ParsePurchasePayload(string payloadJson)
        {
            var payload = JsonUtility.FromJson<PurchaseFailedPayload>(payloadJson);
            var error = JsonUtility.FromJson<ErrorMessage>(payload.errorJson);
            OrderResponseModel order = null;
            if (!string.IsNullOrEmpty(payload.orderJson) && payload.orderJson != "null")
            {
                order = JsonUtility.FromJson<OrderResponseModel>(payload.orderJson);
            }
            return (error, order);
        }
    }

    [Serializable]
    internal class PurchaseFailedPayload
    {
        public string errorJson;
        public string orderJson;
    }
}
