using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Platforms.Android
{
    public static class ConfigModelConverter
    {
        public static AndroidJavaObject ToAndroidJavaObject(string checkoutPublicKey, string customerId, AppDirectEnvironment environment)
        {
            Debug.Log("ToAndroidJavaObject raw values: " + checkoutPublicKey + " " + customerId + " " + environment);
            Debug.Log("ToAndroidJavaObject int env: " + (int)environment);
            return new AndroidJavaObject(
                "com.appcharge.appdirect.domain.model.ConfigModel",
                checkoutPublicKey,
                customerId,
                (int)environment);
        }
    }
}
