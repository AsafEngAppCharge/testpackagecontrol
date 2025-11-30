using System;
using System.IO;
using System.Text.RegularExpressions;
using Appcharge.PaymentLinks.Config;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Appcharge.PaymentLinks.Editor {
    public class AndroidManifestPrebuild : Prebuilder
    {
        public AndroidManifestPrebuild(string path, AppchargePrebuildEditor appchargePrebuildEditor, AppchargeConfig appchargeConfig) 
            : base(path, appchargePrebuildEditor, appchargeConfig)
        {
        }

        public override void Update()
        {
            try
            {
                if (File.Exists(_path))
                {
                    string manifestContent = File.ReadAllText(_path);

                    string packageName = Application.identifier;
                    string gameNameLowerCase = packageName.Split('.')[^1];

                    AppchargeConfig editorConfig = _appchargeConfig;

                    if (!editorConfig.ExcludeInternetPermission && !manifestContent.Contains("android.permission.INTERNET"))
                    {
                        string usesPermission = "<uses-permission android:name=\"android.permission.INTERNET\" />";
                        manifestContent = manifestContent.Insert(manifestContent.IndexOf("<application"), usesPermission + "\n");
                    }

                    if (!editorConfig.ExcludeQueriesBlock && !manifestContent.Contains("<queries>"))
                    {
                        string queries = 
                    @"    <queries>
                            <intent>
                                <action android:name=""android.intent.action.VIEW"" />
                                <data android:scheme=""https"" />
                            </intent>
                        </queries>";
                        int insertIndex = manifestContent.IndexOf("<application");
                        if (insertIndex >= 0)
                        {
                            // Insert without adding an extra newline
                            manifestContent = manifestContent.Insert(insertIndex, queries + "\n");
                        }
                    }

                    // Fix any incorrect acnative schemes to match the package name
                    if (!editorConfig.ExcludeCustomScheme)
                    {
                        manifestContent = FixCustomSchemeIfNeeded(manifestContent, gameNameLowerCase);
                    }

                    if (!editorConfig.ExcludeAppchargeActivity)
                    {
                        manifestContent = AddAppchargeActivity(manifestContent, gameNameLowerCase, editorConfig);
                    }

                    File.WriteAllText(_path, manifestContent);
                    _appchargePrebuildEditor.LogToFile("Final AndroidManifest.xml content:\n" + manifestContent);
                }
                else
                {
                    _appchargePrebuildEditor.LogToFile("AndroidManifest.xml file not found at path: " + _path + "\n", false);
                }
            }
            catch (Exception ex)
            {
                _appchargePrebuildEditor.LogToFile($"Error updating AndroidManifest.xml: {ex.Message}", true);
                throw;
            }
        }

        private string FixCustomSchemeIfNeeded(string manifestContent, string gameNameLowerCase)
        {
            string correctScheme = $"acnative-{gameNameLowerCase}";
            string schemePattern = @"acnative-(\w+)";
            var regex = new Regex(schemePattern);
            var match = regex.Match(manifestContent);
            
            if (match.Success)
            {
                string existingSchemeValue = match.Groups[1].Value;
                if (existingSchemeValue != gameNameLowerCase)
                {
                    // Replace the incorrect scheme with the correct one
                    manifestContent = manifestContent.Replace($"acnative-{existingSchemeValue}", correctScheme);
                    _appchargePrebuildEditor.LogToFile($"Fixed custom scheme from 'acnative-{existingSchemeValue}' to 'acnative-{gameNameLowerCase}' to match package name");
                }
            }
            
            return manifestContent;
        }

        private string AddAppchargeActivity(string manifestContent, string gameNameLowerCase, AppchargeConfig editorConfig)
        {
            if (manifestContent.Contains("com.appcharge.core.CheckoutActivity"))
                return manifestContent;

            string exported = editorConfig.ExcludeExportedAttribute ? "" : "android:exported=\"true\"";
            string discouragedApi = editorConfig.ExcludeDiscouragedApiTool ? "" : "tools:ignore=\"DiscouragedApi\"";
            string intentFilters = string.Empty;

            if (!editorConfig.ExcludeAppchargeActivityIntentFilters)
            {
                string customScheme = editorConfig.ExcludeCustomScheme ? "" : $"<data android:scheme=\"acnative-{gameNameLowerCase}\" />";
                string customHost = editorConfig.ExcludeCustomHost ? "" : "<data android:host=\"action\" />";
                string httpsScheme = editorConfig.ExcludeHttpsSchemeInActivity ? "" : "<data android:scheme=\"https\" />";
                intentFilters = $@"
                    <intent-filter android:autoVerify=""true"">
                        <action android:name=""android.intent.action.VIEW"" />
                        <category android:name=""android.intent.category.DEFAULT"" />
                        <category android:name=""android.intent.category.BROWSABLE"" />
                        {customScheme}
                        {customHost}
                        {httpsScheme}
                    </intent-filter>";
            }

            string newActivity = $@"
            <activity
                android:name=""com.appcharge.core.CheckoutActivity""
                android:theme=""@style/UnityThemeSelector""
                android:launchMode=""standard""
                android:configChanges=""orientation|screenSize""
                android:screenOrientation=""unspecified""
                {exported}
                {discouragedApi}>
                {intentFilters}
            </activity>";

            int appEndIndex = manifestContent.LastIndexOf("</application>");
            return manifestContent.Insert(appEndIndex, newActivity + "\n");
        }
    }
}