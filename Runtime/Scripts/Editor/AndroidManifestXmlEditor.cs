using System;
using System.Xml;

namespace Appcharge.AppDirect.Editor
{
    public static class AndroidManifestXmlEditor
    {
        public const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
        public const string FileProviderClass = "androidx.core.content.FileProvider";
        public const string InstallPermissionActivityClass = "com.appcharge.appdirect.internal.InstallPermissionActivity";

        public static XmlDocument Load(string path)
        {
            var document = new XmlDocument { PreserveWhitespace = false };
            document.Load(path);
            return document;
        }

        public static void Save(XmlDocument document, string path)
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
                document.Save(writer);
        }

        public static void EnsureUsesPermission(XmlDocument document, string permission)
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

        public static void EnsureQueriesViewHttpsIntent(XmlDocument document)
        {
            var manifest = GetManifest(document);
            if (manifest["queries"] != null)
                return;

            var queries = document.CreateElement("queries");
            var intent = document.CreateElement("intent");
            var action = document.CreateElement("action");
            SetAndroidAttribute(action, "name", "android.intent.action.VIEW");
            intent.AppendChild(action);
            var category = document.CreateElement("category");
            SetAndroidAttribute(category, "name", "android.intent.category.BROWSABLE");
            intent.AppendChild(category);
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

        public static void EnsureFileProvider(XmlDocument document)
        {
            var application = GetOrCreateApplication(document);
            if (FindChildByAndroidName(application, "provider", FileProviderClass) != null)
                return;

            var provider = document.CreateElement("provider");
            SetAndroidAttribute(provider, "name", FileProviderClass);
            SetAndroidAttribute(provider, "authorities", "${applicationId}.fileProvider");
            SetAndroidAttribute(provider, "exported", "false");
            SetAndroidAttribute(provider, "grantUriPermissions", "true");
            var meta = document.CreateElement("meta-data");
            SetAndroidAttribute(meta, "name", "android.support.FILE_PROVIDER_PATHS");
            SetAndroidAttribute(meta, "resource", "@xml/paths");
            provider.AppendChild(meta);
            application.AppendChild(provider);
        }

        public static void EnsureInstallPermissionActivity(XmlDocument document)
        {
            var application = GetOrCreateApplication(document);
            if (FindChildByAndroidName(application, "activity", InstallPermissionActivityClass) != null)
                return;

            var activity = document.CreateElement("activity");
            SetAndroidAttribute(activity, "name", InstallPermissionActivityClass);
            SetAndroidAttribute(activity, "exported", "false");
            SetAndroidAttribute(activity, "theme", "@android:style/Theme.Translucent.NoTitleBar");
            application.AppendChild(activity);
        }

        public static void EnsureEngineMetadata(XmlDocument document)
        {
            var application = GetOrCreateApplication(document);
            EnsureMetaData(document, application, EngineMetadataBuild.EngineNameMetaData, "${ENGINE_NAME}");
            EnsureMetaData(document, application, EngineMetadataBuild.EngineVersionMetaData, "${ENGINE_VERSION_NAME}");
            EnsureMetaData(document, application, EngineMetadataBuild.EngineSdkVersionMetaData, "${ENGINE_SDK_VERSION}");
        }

        public static void EnsureNativeInstallEntries(XmlDocument document)
        {
            EnsureUsesPermission(document, "android.permission.INTERNET");
            EnsureUsesPermission(document, "android.permission.ACCESS_NETWORK_STATE");
            EnsureUsesPermission(document, "android.permission.REQUEST_INSTALL_PACKAGES");
            EnsureUsesPermission(document, "android.permission.FOREGROUND_SERVICE");
            EnsureUsesPermission(document, "android.permission.FOREGROUND_SERVICE_DATA_SYNC");
            EnsureQueriesViewHttpsIntent(document);
            EnsureFileProvider(document);
            EnsureInstallPermissionActivity(document);
        }

        private static void EnsureMetaData(XmlDocument document, XmlElement application, string name, string value)
        {
            if (FindMetaDataByName(application, name) != null)
                return;

            var metaData = document.CreateElement("meta-data");
            SetAndroidAttribute(metaData, "name", name);
            SetAndroidAttribute(metaData, "value", value);
            application.AppendChild(metaData);
        }

        private static XmlElement GetManifest(XmlDocument document)
        {
            return document.DocumentElement ?? throw new InvalidOperationException("Android manifest has no root element.");
        }

        private static XmlElement GetOrCreateApplication(XmlDocument document)
        {
            var manifest = GetManifest(document);
            var application = manifest["application"];
            if (application != null)
                return application;

            application = document.CreateElement("application");
            manifest.AppendChild(application);
            return application;
        }

        private static bool HasUsesPermission(XmlDocument document, string permission)
        {
            foreach (XmlNode node in GetManifest(document).ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && element.Name == "uses-permission" && GetAndroidAttribute(element, "name") == permission)
                    return true;
            }

            return false;
        }

        private static XmlElement FindMetaDataByName(XmlElement application, string name)
        {
            return FindChildByAndroidName(application, "meta-data", name);
        }

        private static XmlElement FindChildByAndroidName(XmlElement parent, string localName, string androidName)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                var element = node as XmlElement;
                if (element != null && element.Name == localName && GetAndroidAttribute(element, "name") == androidName)
                    return element;
            }

            return null;
        }

        private static string GetAndroidAttribute(XmlElement element, string localName)
        {
            return element.GetAttribute(localName, AndroidNamespace);
        }

        private static void SetAndroidAttribute(XmlElement element, string localName, string value)
        {
            element.SetAttribute(localName, AndroidNamespace, value);
        }
    }
}
