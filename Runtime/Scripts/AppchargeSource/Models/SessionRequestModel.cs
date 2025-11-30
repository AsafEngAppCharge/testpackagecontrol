using System.Collections.Generic;

namespace Appcharge.PaymentLinks.Models {
    [System.Serializable]
    public class SessionRequestModel
    {
        public CustomerModel customer;
        public PriceDetailModel priceDetails;
        public OfferModel offer;
        public List<OfferItemModel> items;
        public Dictionary<string, string> sessionMetadata;

        public SessionRequestModel(string customerId, string email, float price, string currency, string offerName, string offerSku, string offerAssetUrl, string offerDescription)
        {
            customer = new CustomerModel { id = customerId, email = email };
            priceDetails = new PriceDetailModel { price = price, currency = currency.ToLower() };
            offer = new OfferModel { name = offerName, sku = offerSku, assetUrl = offerAssetUrl, description = offerDescription };
            items = new List<OfferItemModel>();
            sessionMetadata = new Dictionary<string, string>();
        }
    }
}