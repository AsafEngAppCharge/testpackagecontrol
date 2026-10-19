using System;
using System.IO;
using System.Text.RegularExpressions;
using Appcharge.AppDirect.Config;

namespace Appcharge.AppDirect.Editor
{
    public sealed class SyncSdkVersionPrebuild : Prebuilder
    {
        public SyncSdkVersionPrebuild(AppDirectPrebuildEditor prebuildEditor, AppDirectConfig config)
            : base(ResolveSdkVersionPath(), prebuildEditor, config)
        {
        }

        public override void Update()
        {
            try
            {
                string path = ResolveSdkVersionPath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return;

                string version = EngineMetadataBuild.EngineSdkVersion;
                string content = File.ReadAllText(path);
                string updated = Regex.Replace(
                    content,
                    "(public const string UnitySdkVersion = )\"[^\"]*\"",
                    "$1\"" + version + "\"");
                if (updated != content)
                    File.WriteAllText(path, updated);
            }
            catch (Exception ex)
            {
                _prebuildEditor.LogToFile("SyncSdkVersionPrebuild: " + ex.Message, true);
            }
        }

        private static string ResolveSdkVersionPath()
        {
            return Path.Combine(EngineMetadataBuild.GetPackageRoot(), "Runtime", "Scripts", "Config", "SdkVersion.cs");
        }
    }
}
