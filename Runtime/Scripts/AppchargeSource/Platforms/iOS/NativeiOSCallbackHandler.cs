using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.iOS {
    public class NativeiOSCallbackHandler : MonoBehaviour
    {
        private ICheckoutPurchase _checkoutCallback;

        public void Inject(ICheckoutPurchase checkoutCallback)
        {
            _checkoutCallback = checkoutCallback;
        }

        public void OnInitialized()
        {
            _checkoutCallback?.OnInitialized();
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

        public void OnPurchaseFailed(string errorJson)
        {
            var error = JsonUtility.FromJson<ErrorMessage>(errorJson);
            _checkoutCallback?.OnPurchaseFailed(error);
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
}