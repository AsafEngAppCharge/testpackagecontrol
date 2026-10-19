using System;
using System.IO;
using System.Xml;
using Appcharge.AppDirect.Config;

namespace Appcharge.AppDirect.Editor
{
    public sealed class AndroidManifestPrebuild : Prebuilder
    {
        public AndroidManifestPrebuild(string path, AppDirectPrebuildEditor prebuildEditor, AppDirectConfig config)
            : base(path, prebuildEditor, config)
        {
        }

        public override void Update()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    _prebuildEditor.LogToFile("AndroidManifest.xml file not found at path: " + _path, false);
                    return;
                }

                XmlDocument document = AndroidManifestXmlEditor.Load(_path);
                if (_config.EnsureNativeManifestEntries)
                    AndroidManifestXmlEditor.EnsureNativeInstallEntries(document);

                AndroidManifestXmlEditor.EnsureEngineMetadata(document);
                AndroidManifestXmlEditor.Save(document, _path);
                _prebuildEditor.LogToFile("Final AndroidManifest.xml content:\n" + File.ReadAllText(_path));
            }
            catch (Exception ex)
            {
                _prebuildEditor.LogToFile("Error updating AndroidManifest.xml: " + ex.Message, true);
            }
        }
    }
}
