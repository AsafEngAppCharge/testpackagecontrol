using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Appcharge.PaymentLinks.Config;
using UnityEngine;

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
                    string originalGradle = File.ReadAllText(_path);
                    string gradleTemplate = originalGradle;

                    if (!_appchargeConfig.ExcludeBuildConfig && IsUnity6000OrNewer())
                        gradleTemplate = CheckBuildConfigFeature(gradleTemplate);

                    var dependenciesToAdd = new List<(string, string)>
                    {
                        ("implementation 'com.appcharge:android-payment-links:2.0.0'", "com.appcharge:android-payment-links")
                    };
                    
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

                    const string defaultConfigMarker = "defaultConfig {";
                    int dcIndex = gradleTemplate.IndexOf(defaultConfigMarker);
                    if (dcIndex >= 0)
                    {
                        int insertAfterOpen = dcIndex + defaultConfigMarker.Length;
                        string injection = "";
                        if (!gradleTemplate.Contains("buildConfigField \"String\", \"ENGINE_NAME\""))
                        {
                            string engineName = EscapeGradleString(EngineMetadataBuild.EngineName);
                            string engineVersion = EscapeGradleString(Application.unityVersion);
                            string sdkVersion = EscapeGradleString(EngineMetadataBuild.EngineSdkVersion);
                            injection += "\n        buildConfigField \"String\", \"ENGINE_NAME\", \"" + engineName + "\"" +
                                "\n        buildConfigField \"String\", \"ENGINE_VERSION_NAME\", \"" + engineVersion + "\"" +
                                "\n        buildConfigField \"String\", \"ENGINE_SDK_VERSION\", \"" + sdkVersion + "\"";
                        }
                        if (!gradleTemplate.Contains("manifestPlaceholders[\"ENGINE_NAME\"]"))
                        {
                            string n = GroovyDoubleQuotedString(EngineMetadataBuild.EngineName);
                            string v = GroovyDoubleQuotedString(Application.unityVersion);
                            string s = GroovyDoubleQuotedString(EngineMetadataBuild.EngineSdkVersion);
                            injection += "\n        manifestPlaceholders[\"ENGINE_NAME\"] = " + n +
                                "\n        manifestPlaceholders[\"ENGINE_VERSION_NAME\"] = " + v +
                                "\n        manifestPlaceholders[\"ENGINE_SDK_VERSION\"] = " + s;
                        }
                        if (injection.Length > 0)
                            gradleTemplate = gradleTemplate.Insert(insertAfterOpen, injection);
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
                        }
                    }
                    if (gradleTemplate != originalGradle)
                        File.WriteAllText(_path, gradleTemplate);
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

        private static bool IsUnity6000OrNewer()
        {
            string unityVersion = Application.unityVersion;
            int dotIndex = unityVersion.IndexOf('.');
            string majorPart = dotIndex > 0 ? unityVersion.Substring(0, dotIndex) : unityVersion;
            return int.TryParse(majorPart, out int major) && major >= 6000;
        }

        private static bool IsBuildConfigTrue(string gradleTemplate)
        {
            return Regex.IsMatch(gradleTemplate, @"buildConfig\s*=\s*true", RegexOptions.IgnoreCase);
        }

        private static bool IsBuildConfigFalse(string gradleTemplate)
        {
            return Regex.IsMatch(gradleTemplate, @"buildConfig\s*=\s*false", RegexOptions.IgnoreCase);
        }


        private string CheckBuildConfigFeature(string gradleTemplate)
        {
            if (IsBuildConfigFalse(gradleTemplate))
            {
                const string message =
                    "mainTemplate.gradle has buildConfig = false. On Unity 6+, Appcharge SDK requires buildConfig enabled " +
                    "for ENGINE_* buildConfigField entries to compile. The Gradle file was not modified.";
                _appchargePrebuildEditor.LogToFile("Warning: " + message, false);
                Debug.LogWarning("[Appcharge] " + message);
                return gradleTemplate;
            }

            if (IsBuildConfigTrue(gradleTemplate))
                return gradleTemplate;

            const string buildFeaturesMarker = "buildFeatures {";
            int buildFeaturesIndex = gradleTemplate.IndexOf(buildFeaturesMarker, StringComparison.Ordinal);
            if (buildFeaturesIndex >= 0)
            {
                int insertAfterOpen = buildFeaturesIndex + buildFeaturesMarker.Length;
                _appchargePrebuildEditor.LogToFile("Added buildConfig = true to existing buildFeatures block in mainTemplate.gradle for Unity 6+.");
                return gradleTemplate.Insert(insertAfterOpen, "\n        buildConfig = true");
            }

            const string androidMarker = "android {";
            int androidIndex = gradleTemplate.IndexOf(androidMarker, StringComparison.Ordinal);
            if (androidIndex < 0)
            {
                _appchargePrebuildEditor.LogToFile("Warning: 'android {' block not found in mainTemplate.gradle. Cannot add buildFeatures.buildConfig.", false);
                return gradleTemplate;
            }

            int insertAfterAndroidOpen = androidIndex + androidMarker.Length;
            _appchargePrebuildEditor.LogToFile("Added buildFeatures { buildConfig = true } to mainTemplate.gradle for Unity 6+.");
            return gradleTemplate.Insert(insertAfterAndroidOpen, "\n    buildFeatures {\n        buildConfig = true\n    }");
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

        private static string EscapeGradleString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\\\"\\\"";
            return "\\\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\\\"";
        }

        private static string GroovyDoubleQuotedString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$") + "\"";
        }
    }
}