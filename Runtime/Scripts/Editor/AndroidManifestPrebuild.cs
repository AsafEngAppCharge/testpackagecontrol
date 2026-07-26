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
                            manifestContent = manifestContent.Insert(insertIndex, queries + "\n");
                        }
                    }

                    if (!editorConfig.ExcludeCustomScheme)
                    {
                        manifestContent = FixCustomSchemeIfNeeded(manifestContent, gameNameLowerCase);
                    }

                    manifestContent = FixCheckoutActivityIntentFilterIfNeeded(manifestContent);

                    if (!editorConfig.ExcludeAppchargeActivity)
                    {
                        manifestContent = AddAppchargeActivity(manifestContent, gameNameLowerCase, editorConfig);
                    }

                    if (!editorConfig.ExcludeCheckoutService)
                    {
                        manifestContent = AddCheckoutServiceAndPermissions(manifestContent);
                    }

                    manifestContent = AddEngineMetadataMetaData(manifestContent);

                    if (manifestContent.Contains("com.appcharge.core.CheckoutActivity"))
                    {
                        manifestContent = manifestContent.Replace("com.appcharge.core.CheckoutActivity", "com.appcharge.paymentlinks.CheckoutActivity");
                        _appchargePrebuildEditor.LogToFile("Updated legacy activity name from com.appcharge.core.CheckoutActivity to com.appcharge.paymentlinks.CheckoutActivity");
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

        private string FixCheckoutActivityIntentFilterIfNeeded(string manifestContent)
        {
            const string activityMarker = "com.appcharge.paymentlinks.CheckoutActivity";
            if (!manifestContent.Contains(activityMarker))
                return manifestContent;

            int activityIndex = manifestContent.IndexOf(activityMarker, StringComparison.Ordinal);
            int activityStart = manifestContent.LastIndexOf("<activity", activityIndex, StringComparison.Ordinal);
            if (activityStart < 0)
                return manifestContent;

            int activityEnd = manifestContent.IndexOf("</activity>", activityIndex, StringComparison.Ordinal);
            if (activityEnd < 0)
                return manifestContent;
            activityEnd += "</activity>".Length;

            string activityBlock = manifestContent.Substring(activityStart, activityEnd - activityStart);
            string updatedBlock = activityBlock;
            updatedBlock = Regex.Replace(updatedBlock, @"<intent-filter\s+android:autoVerify\s*=\s*""true""\s*>", "<intent-filter>", RegexOptions.IgnoreCase);
            updatedBlock = Regex.Replace(updatedBlock, @"<data\s+android:scheme\s*=\s*""https""\s*/>\s*", string.Empty, RegexOptions.IgnoreCase);

            if (updatedBlock == activityBlock)
                return manifestContent;

            _appchargePrebuildEditor.LogToFile("Updated CheckoutActivity intent-filter for smart deep link handling (removed autoVerify and https scheme).");
            return manifestContent.Substring(0, activityStart) + updatedBlock + manifestContent.Substring(activityEnd);
        }

        private string AddAppchargeActivity(string manifestContent, string gameNameLowerCase, AppchargeConfig editorConfig)
        {
            if (manifestContent.Contains("com.appcharge.paymentlinks.CheckoutActivity"))
                return manifestContent;

            string exported = editorConfig.ExcludeExportedAttribute ? "" : "android:exported=\"true\"";
            string discouragedApi = editorConfig.ExcludeDiscouragedApiTool ? "" : "tools:ignore=\"DiscouragedApi\"";
            string intentFilters = string.Empty;

            if (!editorConfig.ExcludeAppchargeActivityIntentFilters)
            {
                string customScheme = editorConfig.ExcludeCustomScheme ? "" : $"<data android:scheme=\"acnative-{gameNameLowerCase}\" />";
                string customHost = editorConfig.ExcludeCustomHost ? "" : "<data android:host=\"action\" />";
                intentFilters = $@"
                    <intent-filter>
                        <action android:name=""android.intent.action.VIEW"" />
                        <category android:name=""android.intent.category.DEFAULT"" />
                        <category android:name=""android.intent.category.BROWSABLE"" />
                        {customScheme}
                        {customHost}
                    </intent-filter>";
            }

            string newActivity = $@"
            <activity
                android:name=""com.appcharge.paymentlinks.CheckoutActivity""
                android:theme=""@style/UnityThemeSelector""
                android:launchMode=""singleTask""
                android:configChanges=""orientation|screenSize""
                android:screenOrientation=""unspecified""
                {exported}
                {discouragedApi}>
                {intentFilters}
            </activity>";

            int appEndIndex = manifestContent.LastIndexOf("</application>");
            return manifestContent.Insert(appEndIndex, newActivity + "\n");
        }

        private string AddCheckoutServiceAndPermissions(string manifestContent)
        {
            int insertBeforeApplication = manifestContent.IndexOf("<application");
            if (insertBeforeApplication < 0)
                return manifestContent;

            string permissionsToAdd = string.Empty;
            if (!manifestContent.Contains("android.permission.FOREGROUND_SERVICE\" />"))
                permissionsToAdd += "<uses-permission android:name=\"android.permission.FOREGROUND_SERVICE\" />\n";
                
            if (!manifestContent.Contains("android.permission.POST_NOTIFICATIONS"))
                permissionsToAdd += "<uses-permission android:name=\"android.permission.POST_NOTIFICATIONS\" />\n";

            if (permissionsToAdd.Length > 0)
                manifestContent = manifestContent.Insert(insertBeforeApplication, permissionsToAdd);

            if (!manifestContent.Contains("com.appcharge.paymentlinks.CheckoutService"))
            {
                string service = @"
                    <service android:name=""com.appcharge.paymentlinks.CheckoutService"" android:exported=""false"" android:foregroundServiceType=""shortService"" />\n";
                int appEndIndex = manifestContent.LastIndexOf("</application>");
                manifestContent = manifestContent.Insert(appEndIndex, service + "\n");
            }

            return manifestContent;
        }

        /// <summary>Values resolved at merge time via manifestPlaceholders in mainTemplate.gradle (MainTemplatePrebuild).</summary>
        private string AddEngineMetadataMetaData(string manifestContent)
        {
            const string marker = "com.appcharge.paymentlinks.ENGINE_NAME";
            if (manifestContent.Contains(marker))
                return manifestContent;

            int appEndIndex = manifestContent.LastIndexOf("</application>");
            if (appEndIndex < 0)
                return manifestContent;

            string block =
                "        <meta-data\n" +
                "            android:name=\"com.appcharge.paymentlinks.ENGINE_NAME\"\n" +
                "            android:value=\"${ENGINE_NAME}\" />\n" +
                "        <meta-data\n" +
                "            android:name=\"com.appcharge.paymentlinks.ENGINE_VERSION_NAME\"\n" +
                "            android:value=\"${ENGINE_VERSION_NAME}\" />\n" +
                "        <meta-data\n" +
                "            android:name=\"com.appcharge.paymentlinks.ENGINE_SDK_VERSION\"\n" +
                "            android:value=\"${ENGINE_SDK_VERSION}\" />\n";

            return manifestContent.Insert(appEndIndex, block);
        }
    }
}