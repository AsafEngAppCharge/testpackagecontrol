#if UNITY_EDITOR
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Editor.Models {
    [System.Serializable]
    public class OrderValidationResponseModel
    {
        public string state;
        public string reason;

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}
#endif