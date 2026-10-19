#if UNITY_EDITOR
using System;
using Appcharge.AppDirect.Config;
using UnityEditor;
using UnityEngine;

namespace Appcharge.AppDirect.PackageEditor
{
    public sealed class AppDirectConfigBuilder : AssetPostprocessor
    {
        private static bool _hasCheckedConfig;

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool packageImported = false;
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.IndexOf("Appcharge.AppDirect", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    packageImported = true;
                    break;
                }
            }

            if (packageImported || !_hasCheckedConfig)
            {
                _hasCheckedConfig = true;
                EnsureConfigExists();
            }
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                if (_hasCheckedConfig)
                    return;
                _hasCheckedConfig = true;
                EnsureConfigExists();
            };
        }

        private static void EnsureConfigExists()
        {
            if (AssetDatabase.LoadAssetAtPath<AppDirectConfig>(ConfigUtility.AssetPath) != null)
                return;

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Appcharge"))
                AssetDatabase.CreateFolder("Assets/Resources", "Appcharge");

            var config = ScriptableObject.CreateInstance<AppDirectConfig>();
            AssetDatabase.CreateAsset(config, ConfigUtility.AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Created " + ConfigUtility.AssetPath);
        }
    }
}
#endif
