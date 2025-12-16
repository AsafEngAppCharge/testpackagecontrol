using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Appcharge.PaymentLinks.Config {
    public class ConfigUtility
    {
        private static AppchargeConfig _appchargeConfig;
        public static AppchargeConfig GetConfig() {
            if (_appchargeConfig == null) {
                #if UNITY_EDITOR
                _appchargeConfig = AssetDatabase.LoadAssetAtPath<AppchargeConfig>("Assets/Resources/Appcharge/AppchargeConfig.asset");
                
                if (_appchargeConfig == null)
                {
                    _appchargeConfig = Resources.Load<AppchargeConfig>("Appcharge/AppchargeConfig");
                }
                #else
                _appchargeConfig = Resources.Load<AppchargeConfig>("Appcharge/AppchargeConfig");
                #endif

                if (_appchargeConfig == null) {
                    throw new Exception("AppchargeConfig not found. Please create an AppchargeConfig file at 'Assets/Resources/Appcharge/AppchargeConfig.asset'.");
                }
            }

            return _appchargeConfig;
        }
    }
}