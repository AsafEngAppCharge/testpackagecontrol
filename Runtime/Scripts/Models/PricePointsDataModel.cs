namespace Appcharge.PaymentLinks.Models {
    [System.Serializable]
    public class PricePointsDataModel
    {
        public string currencyCode;
        public string currencySymbol;
        public string fractionalSeparator;
        public string milSeparator;
        public string symbolPosition;
        public bool? spacing;
        public int? multiplier;
    }
}