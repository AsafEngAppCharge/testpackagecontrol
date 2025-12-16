namespace Appcharge.PaymentLinks.Models {
    [System.Serializable]
    public class OfferItemModel
    {
        public string Name { get; private set; }
        public string AssetUrl { get; private set; }
        public string Sku { get; private set; }
        public int Quantity { get; private set; }
        public string QuantityDisplay { get; private set; }

        public OfferItemModel(string name, string assetUrl, string sku, int quantity)
        {
            Name = name;
            AssetUrl = assetUrl;
            Sku = sku;
            Quantity = quantity;
            QuantityDisplay = "";
        }

        public OfferItemModel(string name, string assetUrl, string sku, int quantity, string quantityDisplay)
        {
            Name = name;
            AssetUrl = assetUrl;
            Sku = sku;
            Quantity = quantity;
            QuantityDisplay = quantityDisplay;
        }
    }
}
