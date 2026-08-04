using System;
using System.Xml;
using Appcharge.PaymentLinks.Config;
using UnityEngine;

namespace Appcharge.PaymentLinks.Editor {
    internal static class AndroidManifestXmlEditor
    {
        internal const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
        internal const string ToolsNamespace = "http://schemas.android.com/tools";

        internal const string CheckoutActivityClass = "com.appcharge.paymentlinks.CheckoutActivity";
        internal const string LegacyCheckoutActivityClass = "com.appcharge.core.CheckoutActivity";
        internal const string CheckoutServiceClass = "com.appcharge.paymentlinks.CheckoutService";

        internal const string EngineNameMetaData = "com.appcharge.paymentlinks.ENGINE_NAME";

        internal const string ActionView = "android.intent.action.VIEW";
        internal const string CategoryDefault = "android.intent.category.DEFAULT";
        internal const string CategoryBrowsable = "android.intent.category.BROWSABLE";

        internal static XmlDocument Load(string path)
        {
            var document = new XmlDocument { PreserveWhitespace = false };
            document.Load(path);
            return document;
        }

        internal static void Save(XmlDocument document, string path)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = false,
                Encoding = new System.Text.UTF8Encoding(false)
            };
            using (var writer = XmlWriter.Create(path, settings))
            {
                document.Save(writer);
            }
        }

        internal static XmlElement GetManifest(XmlDocument document) =>
            document.DocumentElement ?? throw new InvalidOperationException("Android manifest has no root element.");

        internal static XmlElement GetOrCreateApplication(XmlDocument document)
        {
            var manifest = GetManifest(document);
            var application = manifest["application"];
            if (application != null)
                return application;

            throw new InvalidOperationException("Android manifest is missing a required <application> element.");
        }

        internal static void EnsureUsesPermission(XmlDocument document, string permission)
        {
            if (HasUsesPermission(document, permission))
                return;

            var manifest = GetManifest(document);
            var permissionElement = document.CreateElement("uses-permission");
            SetAndroidAttribute(permissionElement, "name", permission);

            var application = manifest["application"];
            if (application != null)
                manifest.InsertBefore(permissionElement, application);
            else
                manifest.AppendChild(permissionElement);
        }

        internal static void EnsureQueriesViewHttpsIntent(XmlDocument document)
        {
            var manifest = GetManifest(document);
            if (manifest["queries"] != null)
                return;

            var queries = document.CreateElement("queries");
            var intent = document.CreateElement("intent");

            var action = document.CreateElement("action");
            SetAndroidAttribute(action, "name", ActionView);
            intent.AppendChild(action);

            var data = document.CreateElement("data");
            SetAndroidAttribute(data, "scheme", "https");
            intent.AppendChild(data);

            queries.AppendChild(intent);

            var application = manifest["application"];
            if (application != null)
                manifest.InsertBefore(queries, application);
            else
                manifest.AppendChild(queries);
        }

        internal static void FixAcnativeSchemeValues(XmlDocument document, string gameNameLowerCase, Action<string> log)
        {
            string correctScheme = $"acnative-{gameNameLowerCase}";
            foreach (XmlNode node in document.GetElementsByTagName("data"))
            {
                var data = node as XmlElement;
                if (data == null)
                    continue;

                string scheme = GetAndroidAttribute(data, "scheme");
                if (string.IsNullOrEmpty(scheme) || !scheme.StartsWith("acnative-", StringComparison.Ordinal))
                    continue;

                string suffix = scheme.Substring("acnative-".Length);
                if (suffix == gameNameLowerCase)
                    continue;

                SetAndroidAttribute(data, "scheme", correctScheme);
                log?.Invoke($"Fixed custom scheme from '{scheme}' to '{correctScheme}' to match package name");
            }
        }

        /// <summary>Matches master FixCheckoutActivityIntentFilterIfNeeded (25685): strip autoVerify and https on CheckoutActivity intent-filters only.</summary>
        internal static void FixCheckoutActivitySmartDeepLinkIfNeeded(XmlDocument document, Action<string> log)
        {
            var application = GetManifest(document)["application"];
            if (application == null)
                return;

            var activity = FindActivityByName(application, CheckoutActivityClass);
            if (activity == null)
                return;

            bool changed = false;
            foreach (XmlNode node in activity.ChildNodes)
            {
                var intentFilter = node as XmlElement;
                if (intentFilter == null || intentFilter.Name != "intent-filter")
                    continue;

                if (!string.IsNullOrEmpty(GetAndroidAttribute(intentFilter, "autoVerify")))
                {
                    intentFilter.RemoveAttribute("autoVerify", AndroidNamespace);
                    changed = true;
                }

                changed |= RemoveDataElements(intentFilter, scheme: "https", host: null);
            }

            if (changed)
                log?.Invoke("Updated CheckoutActivity intent-filter for smart deep link handling (removed autoVerify and https scheme).");
        }

        internal static void EnsureCheckoutActivity(XmlDocument document, AppchargeConfig config, string gameNameLowerCase, Action<string> log)
        {
            var application = GetOrCreateApplication(document);
            if (FindActivityByName(application, CheckoutActivityClass) != null)
                return;

            var activity = CreateCheckoutActivityElement(document, config);
            application.AppendChild(activity);
            log?.Invoke("Added CheckoutActivity to AndroidManifest.xml");

            if (config.ExcludeAppchargeActivityIntentFilters)
                return;

            AppendNewCheckoutActivityIntentFilter(document, activity, config, gameNameLowerCase);
        }

        internal static void MigrateLegacyCheckoutActivityIfNeeded(XmlDocument document, Action<string> log)
        {
            var application = GetManifest(document)["application"];
            if (application == null)
                return;

            MigrateLegacyCheckoutActivity(application, log);
        }

        internal static void EnsureCheckoutServiceAndPermissions(XmlDocument document)
        {
            EnsureUsesPermission(document, "android.permission.FOREGROUND_SERVICE");
            EnsureUsesPermission(document, "android.permission.POST_NOTIFICATIONS");

            var application = GetOrCreateApplication(document);
            if (FindChildByAndroidName(application, "service", CheckoutServiceClass) != null)
                return;

            var service = document.CreateElement("service");
            SetAndroidAttribute(service, "name", CheckoutServiceClass);
            SetAndroidAttribute(service, "exported", "false");
            SetAndroidAttribute(service, "foregroundServiceType", "shortService");
            application.AppendChild(service);
        }

        /// <summary>Values resolved at merge time via manifestPlaceholders in mainTemplate.gradle (MainTemplatePrebuild).</summary>
        internal static void EnsureEngineMetadata(XmlDocument document)
        {
            var application = GetOrCreateApplication(document);
            if (FindMetaDataByName(application, EngineNameMetaData) != null)
                return;

            AppendMetaData(document, application, EngineNameMetaData, "${ENGINE_NAME}");
            AppendMetaData(document, application, "com.appcharge.paymentlinks.ENGINE_VERSION_NAME", "${ENGINE_VERSION_NAME}");
            AppendMetaData(document, application, "com.appcharge.paymentlinks.ENGINE_SDK_VERSION", "${ENGINE_SDK_VERSION}");
        }

        private static void MigrateLegacyCheckoutActivity(XmlElement application, Action<string> log)
        {
            var legacy = FindActivityByName(application, LegacyCheckoutActivityClass);
            if (legacy == null)
                return;

            SetAndroidAttribute(legacy, "name", CheckoutActivityClass);
            log?.Invoke($"Updated legacy activity name from {LegacyCheckoutActivityClass} to {CheckoutActivityClass}");
        }

        private static XmlElement CreateCheckoutActivityElement(XmlDocument document, AppchargeConfig config)
        {
            var activity = document.CreateElement("activity");
            SetAndroidAttribute(activity, "name", CheckoutActivityClass);
            ApplyCheckoutActivityAttributes(activity, config);
            return activity;
        }

        private static void ApplyCheckoutActivityAttributes(XmlElement activity, AppchargeConfig config)
        {
            SetAndroidAttribute(activity, "name", CheckoutActivityClass);
            SetAndroidAttribute(activity, "theme", "@style/UnityThemeSelector");
            SetAndroidAttribute(activity, "launchMode", "singleTask");
            SetAndroidAttribute(activity, "configChanges", "orientation|screenSize");
            SetAndroidAttribute(activity, "screenOrientation", "unspecified");

            if (config.ExcludeExportedAttribute)
                activity.RemoveAttribute("exported", AndroidNamespace);
            else
                SetAndroidAttribute(activity, "exported", "true");

            if (config.ExcludeDiscouragedApiTool)
                activity.RemoveAttribute("ignore", ToolsNamespace);
            else
                activity.SetAttribute("ignore", ToolsNamespace, "DiscouragedApi");
        }

        private static void AppendNewCheckoutActivityIntentFilter(
            XmlDocument document,
            XmlElement activity,
            AppchargeConfig config,
            string gameNameLowerCase)
        {
            var intentFilter = document.CreateElement("intent-filter");

            var action = document.CreateElement("action");
            SetAndroidAttribute(action, "name", ActionView);
            intentFilter.AppendChild(action);

            var categoryDefault = document.CreateElement("category");
            SetAndroidAttribute(categoryDefault, "name", CategoryDefault);
            intentFilter.AppendChild(categoryDefault);

            var categoryBrowsable = document.CreateElement("category");
            SetAndroidAttribute(categoryBrowsable, "name", CategoryBrowsable);
            intentFilter.AppendChild(categoryBrowsable);

            if (!config.ExcludeCustomScheme)
            {
                var schemeData = document.CreateElement("data");
                SetAndroidAttribute(schemeData, "scheme", $"acnative-{gameNameLowerCase}");
                intentFilter.AppendChild(schemeData);
            }

            if (!config.ExcludeCustomHost)
            {
                var hostData = document.CreateElement("data");
                SetAndroidAttribute(hostData, "host", "action");
                intentFilter.AppendChild(hostData);
            }

            activity.AppendChild(intentFilter);
        }

        private static bool RemoveDataElements(XmlElement intentFilter, string scheme, string host)
        {
            var toRemove = new System.Collections.Generic.List<XmlNode>();
            foreach (XmlNode node in intentFilter.ChildNodes)
            {
                var data = node as XmlElement;
                if (data == null || data.Name != "data")
                    continue;

                if (scheme != null && GetAndroidAttribute(data, "scheme") == scheme)
                    toRemove.Add(node);
                else if (host != null && GetAndroidAttribute(data, "host") == host)
                    toRemove.Add(node);
            }

            foreach (var node in toRemove)
                intentFilter.RemoveChild(node);

            return toRemove.Count > 0;
        }

        private static void AppendMetaData(XmlDocument document, XmlElement application, string name, string value)
        {
            var metaData = document.CreateElement("meta-data");
            SetAndroidAttribute(metaData, "name", name);
            SetAndroidAttribute(metaData, "value", value);
            application.AppendChild(metaData);
        }

        private static XmlElement FindMetaDataByName(XmlElement application, string name)
        {
            foreach (XmlNode node in application.ChildNodes)
            {
                var metaData = node as XmlElement;
                if (metaData != null && metaData.Name == "meta-data" &&
                    GetAndroidAttribute(metaData, "name") == name)
                    return metaData;
            }

            return null;
        }

        private static XmlElement FindActivityByName(XmlElement application, string activityClassName) =>
            FindChildByAndroidName(application, "activity", activityClassName);

        private static XmlElement FindChildByAndroidName(XmlElement parent, string localName, string androidName)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && element.Name == localName &&
                    GetAndroidAttribute(element, "name") == androidName)
                    return element;
            }

            return null;
        }

        private static bool HasUsesPermission(XmlDocument document, string permission)
        {
            foreach (XmlNode node in GetManifest(document).ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && element.Name == "uses-permission" &&
                    GetAndroidAttribute(element, "name") == permission)
                    return true;
            }

            return false;
        }

        private static string GetAndroidAttribute(XmlElement element, string localName) =>
            element.GetAttribute(localName, AndroidNamespace);

        private static void SetAndroidAttribute(XmlElement element, string localName, string value) =>
            element.SetAttribute(localName, AndroidNamespace, value);
    }
}
