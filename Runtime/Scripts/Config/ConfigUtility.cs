using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Appcharge.AppDirect.Config
{
    public static class ConfigUtility
    {
        public const string AssetPath = "Assets/Resources/Appcharge/AppDirectConfig.asset";
        public const string ResourcesPath = "Appcharge/AppDirectConfig";

        private static AppDirectConfig _config;

        public static AppDirectConfig GetConfig()
        {
            if (_config != null)
                return _config;

#if UNITY_EDITOR
            _config = AssetDatabase.LoadAssetAtPath<AppDirectConfig>(AssetPath);
            if (_config == null)
                _config = Resources.Load<AppDirectConfig>(ResourcesPath);
#else
            _config = Resources.Load<AppDirectConfig>(ResourcesPath);
#endif
            if (_config == null)
                throw new Exception("AppDirectConfig not found. Create one at '" + AssetPath + "'.");

            return _config;
        }
    }
}
