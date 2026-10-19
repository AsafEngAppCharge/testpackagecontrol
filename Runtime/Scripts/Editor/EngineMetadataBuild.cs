using System;
using System.IO;
using System.Text.RegularExpressions;
using Appcharge.AppDirect.Config;
using UnityEngine;
using UnityEditor;

namespace Appcharge.AppDirect.Editor
{
    public static class EngineMetadataBuild
    {
        public const string EngineName = "unity";
        public const string EngineNameMetaData = "com.appcharge.appdirect.ENGINE_NAME";
        public const string EngineVersionMetaData = "com.appcharge.appdirect.ENGINE_VERSION_NAME";
        public const string EngineSdkVersionMetaData = "com.appcharge.appdirect.ENGINE_SDK_VERSION";

        public static string EngineSdkVersion
        {
            get
            {
                string path = GetPackageJsonPath();
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        var match = Regex.Match(json, "\\\"version\\\"\\s*:\\s*\\\"([^\\\"]+)\\\"");
                        if (match.Success)
                            return match.Groups[1].Value;
                    }
                    catch (Exception)
                    {
                    }
                }
                return SdkVersion.UnitySdkVersion;
            }
        }

        public static string GetPackageRoot()
        {
            var info = PackageInfoForAssembly();
            if (info != null && !string.IsNullOrEmpty(info.resolvedPath))
                return info.resolvedPath;

            string inAssets = Path.Combine(Application.dataPath, "Packages", "com.appcharge.appdirect");
            if (File.Exists(Path.Combine(inAssets, "package.json")))
                return inAssets;

            string inPackages = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages", "com.appcharge.appdirect"));
            if (File.Exists(Path.Combine(inPackages, "package.json")))
                return inPackages;

            return inAssets;
        }

        private static string GetPackageJsonPath()
        {
            return Path.Combine(GetPackageRoot(), "package.json");
        }

        private static UnityEditor.PackageManager.PackageInfo PackageInfoForAssembly()
        {
            return UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(EngineMetadataBuild).Assembly);
        }
    }
}
