using System.Collections.Generic;
using Appcharge.PaymentLinks.Models;

namespace Appcharge.PaymentLinks.Interfaces {
    public interface ICheckoutPurchase
    {
        void OnPurchaseSuccess(OrderResponseModel order);
        void OnPurchaseFailed(ErrorMessage error, OrderResponseModel order);
        void OnInitialized();
        void OnInitializeFailed(ErrorMessage error);
        void OnPricePointsSuccess(PricePointsModel pricePoints);
        void OnPricePointsFail(ErrorMessage error);
    }
}
