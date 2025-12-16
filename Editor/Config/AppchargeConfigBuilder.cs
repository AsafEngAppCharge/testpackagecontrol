#if UNITY_EDITOR
using System;
using Appcharge.PaymentLinks.Config;
using UnityEditor;
using UnityEngine;

public class AppchargeConfigBuilder : AssetPostprocessor
{
    private const string ConfigPath = "Assets/Resources/Appcharge/AppchargeConfig.asset";
    private const string ResourcesFolderPath = "Assets/Resources/Appcharge";
    private const string TargetDllName = "Appcharge.PaymentLinks.dll";
    private static bool _hasCheckedConfig = false;

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // Check if the package DLL was imported (works for both UPM and .unitypackage)
        bool packageDllImported = false;
        foreach (string assetPath in importedAssets)
        {
            // Check if path contains the target DLL name (handles both Assets/ and Packages/ paths)
            if (assetPath.Contains(TargetDllName, StringComparison.OrdinalIgnoreCase))
            {
                packageDllImported = true;
                break;
            }
        }

        if (packageDllImported || !_hasCheckedConfig)
        {
            _hasCheckedConfig = true;
            EnsureConfigExists();
        }
    }

    [InitializeOnLoadMethod]
    static void InitializeOnLoad()
    {
        // Also check when Unity loads to catch initial package import
        EditorApplication.delayCall += () =>
        {
            if (!_hasCheckedConfig)
            {
                _hasCheckedConfig = true;
                EnsureConfigExists();
            }
        };
    }

    private static void EnsureConfigExists()
    {
        // Check if config already exists
        if (AssetDatabase.LoadAssetAtPath<AppchargeConfig>(ConfigPath) != null)
        {
            return;
        }

        try
        {
            // Ensure the Resources/Appcharge directory exists
            string resourcesPath = "Assets/Resources";
            string appchargePath = ResourcesFolderPath;
            
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            if (!AssetDatabase.IsValidFolder(appchargePath))
            {
                AssetDatabase.CreateFolder(resourcesPath, "Appcharge");
            }

            // Create the config asset
            AppchargeConfig config = ScriptableObject.CreateInstance<AppchargeConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[Appcharge] Created AppchargeConfig at {ConfigPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Appcharge] Failed to create AppchargeConfig at {ConfigPath}: {ex.Message}");
        }
    }
}
#endif