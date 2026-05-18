using System;
using System.IO;
using System.Text.RegularExpressions;
using Appcharge.PaymentLinks.Config;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Appcharge.PaymentLinks.Editor
{
    /// <summary>Syncs SdkVersion.UnitySdkVersion in Config from package.json so Runtime (iOS, Editor) use the same version.</summary>
    public class SyncSdkVersionPrebuild : Prebuilder
    {
        public SyncSdkVersionPrebuild(string pathToSdkVersionCs, AppchargePrebuildEditor appchargePrebuildEditor, AppchargeConfig appchargeConfig)
            : base(pathToSdkVersionCs, appchargePrebuildEditor, appchargeConfig)
        {
        }

        public override void Update()
        {
#if UNITY_EDITOR
            try
            {
                string path = ResolveSdkVersionPath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return;
                string version = EngineMetadataBuild.EngineSdkVersion;
                string content = File.ReadAllText(path);
                string updated = Regex.Replace(content, @"(public const string UnitySdkVersion = )""[^""]*""", "$1\"" + version + "\"");
                if (updated != content)
                    File.WriteAllText(path, updated);
            }
            catch (Exception ex)
            {
                _appchargePrebuildEditor.LogToFile($"SyncSdkVersionPrebuild: {ex.Message}", true);
            }
#endif
        }

#if UNITY_EDITOR
        private static string ResolveSdkVersionPath()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.appcharge.paymentlinks");
            if (info != null && !string.IsNullOrEmpty(info.resolvedPath))
                return Path.Combine(info.resolvedPath, "Runtime", "Scripts", "Config", "SdkVersion.cs");
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages", "com.appcharge.paymentlinks", "Runtime", "Scripts", "Config", "SdkVersion.cs"));
        }
#endif
    }
}
