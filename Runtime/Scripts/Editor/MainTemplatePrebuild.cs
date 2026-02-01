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

                    if (!_appchargeConfig.ExcludeCoreKtx)
                        dependenciesToAdd.Add(("implementation 'androidx.core:core-ktx:1.13.1'", "androidx.core:core-ktx"));

                    if (!_appchargeConfig.ExcludeActivityKtx)
                        dependenciesToAdd.Add(("implementation 'androidx.activity:activity-ktx:1.3.0'", "androidx.activity:activity-ktx"));

                    if (!_appchargeConfig.ExcludeAndroidXBrowser)
                        dependenciesToAdd.Add(("implementation 'androidx.browser:browser:1.8.0'", "androidx.browser:browser"));

                    if (!_appchargeConfig.ExcludeAndroidBrowserHelper)
                        dependenciesToAdd.Add(("implementation 'com.google.androidbrowserhelper:androidbrowserhelper:2.4.0'", "com.google.androidbrowserhelper:androidbrowserhelper"));

                    if (!_appchargeConfig.ExcludeKotlinSerializationJson)
                        dependenciesToAdd.Add(("implementation 'org.jetbrains.kotlinx:kotlinx-serialization-json:1.5.1'", "org.jetbrains.kotlinx:kotlinx-serialization-json"));

                    if (!_appchargeConfig.ExcludeKotlinCoroutinesCore)
                        dependenciesToAdd.Add(("implementation 'org.jetbrains.kotlinx:kotlinx-coroutines-core:1.7.1'", "org.jetbrains.kotlinx:kotlinx-coroutines-core"));

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
                        int insertIndex = -1;
                        
                        int depsMarkerIndex = gradleTemplate.IndexOf("**DEPS**");
                        if (depsMarkerIndex >= 0)
                        {
                            insertIndex = depsMarkerIndex + "**DEPS**".Length;
                        }
                        else
                        {
                            _appchargePrebuildEditor.LogToFile("Warning: '**DEPS**' marker not found in mainTemplate.gradle. Falling back to adding dependencies at the last dependencies block.", false);

                            int depsBlockIndex = gradleTemplate.LastIndexOf("dependencies {");
                            if (depsBlockIndex >= 0)
                            {
                                insertIndex = FindBlockEnd(gradleTemplate, depsBlockIndex);
                            }
                            else
                            {
                                _appchargePrebuildEditor.LogToFile("Warning: No 'dependencies {' block found in mainTemplate.gradle. Cannot add dependencies.", false);
                            }
                        }
                        
                        if (insertIndex >= 0)
                        {
                            string dependenciesToInsert = "\n" + string.Join("\n", missingDependencies) + "\n";
                            gradleTemplate = gradleTemplate.Insert(insertIndex, dependenciesToInsert);
                            File.WriteAllText(_path, gradleTemplate);
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

        private int FindBlockEnd(string text, int blockStart)
        {
            if (blockStart < 0) return -1;
            int braceCount = 0;
            for (int i = blockStart; i < text.Length; i++)
            {
                if (text[i] == '{') braceCount++;
                else if (text[i] == '}')
                {
                    braceCount--;
                    if (braceCount == 0) return i;
                }
            }
            return -1;
        }
    }
}