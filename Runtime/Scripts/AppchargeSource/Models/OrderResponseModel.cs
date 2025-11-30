using UnityEngine;

namespace Appcharge.PaymentLinks.Models {
    [System.Serializable]
    public class OrderResponseModel
    {
        public string currency;
        public string sessionToken;
        public string customerId;
        public string purchaseId;
        public string paymentMethodName;
        public ProductModel[] items;
        public string offerSku;
        public int price;
        public string offerName;
        public long date;
        public string customerCountry;
        public string orderId;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}