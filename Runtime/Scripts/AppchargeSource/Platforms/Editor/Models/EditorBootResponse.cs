#if UNITY_EDITOR
namespace Appcharge.PaymentLinks.Platforms.Editor.Models {
    [System.Serializable]
    public class EditorBootResponse
    {
        public string appchargeUrl;
        public string sessionPath;
        public string pricePointsPath;
        public string wvUrl;
        public string cancelPath;
        public string orderAwardAckPath;
        public string orderNoneAwardAckPath;
        public bool fullscreenMode;
        public BootLoggerModel logger;
        public int orderValidationTimeout;
        public int orderValidationRate;
        public string getOrderPath;
        public int orderValidationRetry;
        public string browserPresentationType;
    }
    
    [System.Serializable]
    public class BootLoggerModel
    {
        public string type;
        public string logUrl;
        public string severity;
        public string eventsUrl;
    }
}
#endif