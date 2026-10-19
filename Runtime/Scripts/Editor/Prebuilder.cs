using System.IO;
using Appcharge.AppDirect.Config;
using UnityEngine;

namespace Appcharge.AppDirect.Editor
{
    public abstract class Prebuilder
    {
        protected string _path;
        protected string _backupPath;
        protected AppDirectPrebuildEditor _prebuildEditor;
        protected AppDirectConfig _config;
        private const string BackupPathExtension = ".bak";
        private readonly string _fileName;

        protected Prebuilder(string path, AppDirectPrebuildEditor prebuildEditor, AppDirectConfig config)
        {
            _path = path;
            _prebuildEditor = prebuildEditor;
            _config = config;
            _fileName = Path.GetFileName(_path);
        }

        public virtual void Backup()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            _backupPath = Path.Combine(projectRoot, "AppDirect", _fileName + BackupPathExtension);
            string directoryPath = Path.GetDirectoryName(_backupPath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            if (!File.Exists(_path))
            {
                Debug.LogWarning("Backup of " + _fileName + " skipped because the original file doesn't exist at path: " + _path);
                return;
            }

            File.Copy(_path, _backupPath, overwrite: true);
        }

        public abstract void Update();
    }
}
