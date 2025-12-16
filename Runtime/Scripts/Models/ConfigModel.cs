using UnityEngine;

namespace Appcharge.PaymentLinks.Models {
    
    [System.Serializable]
    public class ConfigModel
    {
        public string checkoutPublicKey;
        public string environment;
        public string redirectUrl;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}