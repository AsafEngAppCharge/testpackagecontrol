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
            var (error, order) = PurchaseResultPayloadParser.Parse(payloadJson);
            _platform.Callback?.OnPurchaseCanceled(error, order);
        }

        public void OnPurchaseFailed(string payloadJson)
        {
            var (error, order) = PurchaseResultPayloadParser.Parse(payloadJson);
            _platform.Callback?.OnPurchaseFailed(error, order);
        }
    }
}
