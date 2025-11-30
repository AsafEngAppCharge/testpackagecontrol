using System;
using System.Collections.Generic;
using System.IO;
using Appcharge.PaymentLinks.Config;

namespace Appcharge.PaymentLinks.Editor {
    public class GradleTemplatePrebuild : Prebuilder
    {
        public GradleTemplatePrebuild(string path, AppchargePrebuildEditor appchargePrebuildEditor, AppchargeConfig appchargeConfig) : base(path, appchargePrebuildEditor, appchargeConfig)
        {
        }

        public override void Update()
        {
            try
            {
                if (File.Exists(_path))
                {
                    string[] lines = File.ReadAllLines(_path);

                    string useAndroidX = "android.useAndroidX=true";
                    string enableJetifier = "android.enableJetifier=true";

                    AppchargeConfig editorConfig = _appchargeConfig;

                    List<string> modifiedLines = new();

                    foreach (string line in lines)
                    {
                        if (!line.Trim().StartsWith("android.useAndroidX") && !line.Trim().StartsWith("android.enableJetifier"))
                        {
                            modifiedLines.Add(line);
                        }
                    }

                    if (!editorConfig.ExcludeAndroidX)
                    {
                        modifiedLines.Add(useAndroidX);
                    }

                    if (!editorConfig.ExcludeJetifier)
                    {
                        modifiedLines.Add(enableJetifier);
                    }

                    File.WriteAllLines(_path, modifiedLines);
                    string finalContent = File.ReadAllText(_path);
                    _appchargePrebuildEditor.LogToFile("Final gradle.properties content:\n" + finalContent);
                }
                else
                {
                    _appchargePrebuildEditor.LogToFile("gradle.properties file not found at " + _path, false);
                }
            }
            catch (Exception ex)
            {
                _appchargePrebuildEditor.LogToFile($"Error updating gradle.properties: {ex.Message}", true);
            }
        }
    }
}