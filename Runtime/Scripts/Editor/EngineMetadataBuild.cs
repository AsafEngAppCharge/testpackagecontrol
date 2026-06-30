using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Appcharge.PaymentLinks.Editor
{
    /// <summary>
    /// Single source for engine metadata injected at build time into Android (buildConfigField) and iOS (ACPaymentLinks.plist).
    /// Native plugins read these; Unity runtime no longer passes them to init.
    /// </summary>
    public static class EngineMetadataBuild
    {
        public const string EngineName = "unity";

        private const string FallbackSdkVersion = "2.6.0";

        /// <summary>Read from package.json version; falls back to FallbackSdkVersion if unreadable.</summary>
        public static string EngineSdkVersion
        {
            get
            {
#if UNITY_EDITOR
                string path = GetPackageJsonPath();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        var m = Regex.Match(json, @"\""version\""\s*:\s*\""([^""]+)\""");
                        if (m.Success) return m.Groups[1].Value;
                    }
                    catch { }
                }
                return FallbackSdkVersion;
#else
                return FallbackSdkVersion;
#endif
            }
        }

        public static string EngineVersionName
        {
            get
            {
#if UNITY_EDITOR
                return Application.unityVersion;
#else
                return "";
#endif
            }
        }

#if UNITY_EDITOR
        private static string GetPackageJsonPath()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.appcharge.paymentlinks");
            if (info != null && !string.IsNullOrEmpty(info.resolvedPath))
                return Path.Combine(info.resolvedPath, "package.json");
            return Path.Combine(Application.dataPath, "..", "Packages", "com.appcharge.paymentlinks", "package.json");
        }
#endif
    }
}
