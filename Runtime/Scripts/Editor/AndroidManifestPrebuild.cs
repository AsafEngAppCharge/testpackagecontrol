using System;
using System.IO;
using System.Xml;
using Appcharge.PaymentLinks.Config;
using UnityEngine;

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
                if (!File.Exists(_path))
                {
                    _appchargePrebuildEditor.LogToFile("AndroidManifest.xml file not found at path: " + _path + "\n", false);
                    return;
                }

                string packageName = Application.identifier;
                string gameNameLowerCase = packageName.Split('.')[^1];
                AppchargeConfig editorConfig = _appchargeConfig;

                XmlDocument document = AndroidManifestXmlEditor.Load(_path);

                if (!editorConfig.ExcludeInternetPermission)
                    AndroidManifestXmlEditor.EnsureUsesPermission(document, "android.permission.INTERNET");

                if (!editorConfig.ExcludeQueriesBlock)
                    AndroidManifestXmlEditor.EnsureQueriesViewHttpsIntent(document);

                if (!editorConfig.ExcludeAppchargeActivity)
                    AndroidManifestXmlEditor.EnsureCheckoutActivity(document, editorConfig, gameNameLowerCase, LogInfo);

                if (!editorConfig.ExcludeCheckoutService)
                    AndroidManifestXmlEditor.EnsureCheckoutServiceAndPermissions(document);

                AndroidManifestXmlEditor.EnsureEngineMetadata(document);

                AndroidManifestXmlEditor.Save(document, _path);
                _appchargePrebuildEditor.LogToFile("Final AndroidManifest.xml content:\n" + File.ReadAllText(_path));
            }
            catch (Exception ex)
            {
                _appchargePrebuildEditor.LogToFile($"Error updating AndroidManifest.xml: {ex.Message}");
                throw;
            }
        }

        private void LogInfo(string message) => _appchargePrebuildEditor.LogToFile(message);
    }
}
