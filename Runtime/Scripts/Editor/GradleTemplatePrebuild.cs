using System;
using System.Collections.Generic;
using System.IO;
using Appcharge.AppDirect.Config;

namespace Appcharge.AppDirect.Editor
{
    public sealed class GradleTemplatePrebuild : Prebuilder
    {
        private const string UseAndroidXPrefix = "android.useAndroidX";
        private const string EnableJetifierPrefix = "android.enableJetifier";
        private const string UseAndroidXTrue = "android.useAndroidX=true";
        private const string EnableJetifierTrue = "android.enableJetifier=true";

        public GradleTemplatePrebuild(string path, AppDirectPrebuildEditor prebuildEditor, AppDirectConfig config)
            : base(path, prebuildEditor, config)
        {
        }

        public override void Update()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    _prebuildEditor.LogToFile("gradleTemplate.properties file not found at " + _path, false);
                    return;
                }

                string[] lines = File.ReadAllLines(_path);
                var modified = new List<string>();
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    if (!trimmed.StartsWith(UseAndroidXPrefix) && !trimmed.StartsWith(EnableJetifierPrefix))
                        modified.Add(line);
                }

                if (!_config.ExcludeAndroidX)
                    modified.Add(UseAndroidXTrue);
                if (!_config.ExcludeJetifier)
                    modified.Add(EnableJetifierTrue);

                File.WriteAllLines(_path, modified);
                _prebuildEditor.LogToFile("Final gradle.properties content:\n" + File.ReadAllText(_path));
            }
            catch (Exception ex)
            {
                _prebuildEditor.LogToFile("Error updating gradle.properties: " + ex.Message, true);
            }
        }
    }
}
