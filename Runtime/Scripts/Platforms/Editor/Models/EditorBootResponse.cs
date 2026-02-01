#if UNITY_EDITOR
namespace Appcharge.PaymentLinks.Platforms.Editor.Models {
    [System.Serializable]
    public class EditorBootResponse
    {
        public BootPaths paths;
        public BootSettings settings;
        public BootValidations validations;
        
        // Backward compatibility properties - map to new structure
        public string appchargeUrl => paths?.baseUrl ?? string.Empty;
        public string pricePointsPath => paths?.pricePointsPath ?? string.Empty;
        public string getOrderPath => paths?.getOrderPath ?? string.Empty;
        public string cancelPath => paths?.cancelPath ?? string.Empty;
        public string wvUrl => paths?.wrapperUrl ?? string.Empty;
        public BootLoggerModel logger => settings?.logs != null ? new BootLoggerModel
        {
            logUrl = paths?.logUrl ?? string.Empty,
            eventsUrl = paths?.eventsUrl ?? string.Empty,
            severity = settings.logs.severity ?? string.Empty
        } : null;
        public int orderValidationTimeout => settings?.network?.orderValidation?.timeout ?? 90000;
        public int orderValidationRate => settings?.network?.orderValidation?.rate ?? 600;
        public int orderValidationRetry => settings?.network?.orderValidation?.retry ?? 6;
        public string browserPresentationType => settings?.presentation?.browserMode ?? string.Empty;
    }
    
    [System.Serializable]
    public class BootPaths
    {
        public string baseUrl;
        public string cancelPath;
        public string pricePointsPath;
        public string getOrderPath;
        public string manageSubscriptionsPath;
        public string wrapperUrl;
        public string logUrl;
        public string eventsUrl;
    }
    
    [System.Serializable]
    public class BootSettings
    {
        public BootPresentation presentation;
        public BootNetwork network;
        public BootLogs logs;
    }
    
    [System.Serializable]
    public class BootPresentation
    {
        public bool overrideConfig;
        public string browserMode;
        public bool useCheckoutWrapper;
    }
    
    [System.Serializable]
    public class BootNetwork
    {
        public BootOrderValidation orderValidation;
        public BootConnection connection;
    }
    
    [System.Serializable]
    public class BootOrderValidation
    {
        public int timeout;
        public int rate;
        public int retry;
    }
    
    [System.Serializable]
    public class BootConnection
    {
        public int requestTimeout;
        public int responseTimeout;
    }
    
    [System.Serializable]
    public class BootLogs
    {
        public string severity;
    }
    
    [System.Serializable]
    public class BootValidations
    {
        public string[] purchaseId;
    }
    
    // Legacy model for backward compatibility
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