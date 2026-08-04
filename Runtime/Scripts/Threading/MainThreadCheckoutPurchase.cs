using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;

namespace Appcharge.PaymentLinks.Threading {
    public class MainThreadCheckoutPurchase : ICheckoutPurchase {
        private readonly ICheckoutPurchase _inner;

        public MainThreadCheckoutPurchase(ICheckoutPurchase inner) {
            _inner = inner;
        }

        public void OnInitialized() {
            MainThreadDispatcher.Run(() => _inner?.OnInitialized());
        }

        public void OnInitializeFailed(ErrorMessage error) {
            MainThreadDispatcher.Run(() => _inner?.OnInitializeFailed(error));
        }

        public void OnPurchaseSuccess(OrderResponseModel order) {
            MainThreadDispatcher.Run(() => _inner?.OnPurchaseSuccess(order));
        }

        public void OnPurchaseCanceled(ErrorMessage error, OrderResponseModel order) {
            MainThreadDispatcher.Run(() => _inner?.OnPurchaseCanceled(error, order));
        }

        public void OnPurchaseFailed(ErrorMessage error, OrderResponseModel order) {
            MainThreadDispatcher.Run(() => _inner?.OnPurchaseFailed(error, order));
        }
    }
}
