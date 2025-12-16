using System;
using System.Collections.Generic;
using System.IO;
using Appcharge.PaymentLinks.Config;

namespace Appcharge.PaymentLinks.Editor {
    public class MainTemplatePrebuild : Prebuilder
    {
        public MainTemplatePrebuild(string path, AppchargePrebuildEditor appchargePrebuildEditor, AppchargeConfig appchargeConfig) : base(path, appchargePrebuildEditor, appchargeConfig)
        {
        }

        public override void Update()
        {
            try {
                if (File.Exists(_path))
                {
                    string gradleTemplate = File.ReadAllText(_path);

                    var dependenciesToAdd = new List<(string, string)>();

                    if (!_appchargeConfig.ExcludeAppcompat)
                        dependenciesToAdd.Add(("implementation 'androidx.appcompat:appcompat:1.3.1'", "androidx.appcompat:appcompat"));

                    if (!_appchargeConfig.ExcludeAndroidbrowser)
                        dependenciesToAdd.Add(("implementation 'com.google.androidbrowserhelper:androidbrowserhelper:2.4.0'", "com.google.androidbrowserhelper:androidbrowserhelper"));

                    if (!_appchargeConfig.ExcludeKotlin)
                        dependenciesToAdd.AddRange(new (string, string)[]
                        {
                            ("implementation 'org.jetbrains.kotlinx:kotlinx-coroutines-core:1.7.1'", "org.jetbrains.kotlinx:kotlinx-coroutines-core"),
                            ("implementation 'org.jetbrains.kotlinx:kotlinx-serialization-json:1.5.1'", "org.jetbrains.kotlinx:kotlinx-serialization-json"),
                        });

                    var finalDependencies = dependenciesToAdd.ToArray();

                    List<string> missingDependencies = new List<string>();
                    foreach (var (dependency, identifier) in finalDependencies)
                    {
                        if (!gradleTemplate.Contains(identifier))
                        {
                            missingDependencies.Add(dependency);
                        }
                    }

                    if (missingDependencies.Count > 0)
                    {
                        int index = gradleTemplate.IndexOf("dependencies {");
                        if (index >= 0)
                        {
                            int endIndex = gradleTemplate.IndexOf("}", index);
                            if (endIndex >= 0)
                            {
                                string dependenciesToInsert = "\n" + string.Join("\n", missingDependencies) + "\n";
                                gradleTemplate = gradleTemplate.Insert(endIndex, dependenciesToInsert);
                                File.WriteAllText(_path, gradleTemplate);
                            }
                        }
                    }
                    _appchargePrebuildEditor.LogToFile("Final mainTemplate.gradle content:\n" + gradleTemplate);
                }
                else
                {
                    _appchargePrebuildEditor.LogToFile("mainTemplate.gradle file not found at path: " + _path, false);
                }
            }
            catch (Exception ex)
            {
                _appchargePrebuildEditor.LogToFile($"Error updating mainTemplate.gradle: {ex.Message}", true);
            }    
        }
    }
}