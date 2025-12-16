using System.IO;
using Appcharge.PaymentLinks.Config;
using UnityEngine;

namespace Appcharge.PaymentLinks.Editor {
    public abstract class Prebuilder
    {
        protected string _path;
        protected string _backupPath;
        protected AppchargePrebuildEditor _appchargePrebuildEditor;
        protected AppchargeConfig _appchargeConfig;
        private const string _backupPathExtension = ".bak";
        private const string _metaPathExtension = ".meta";
        private string _fileName;

        public Prebuilder(string path, AppchargePrebuildEditor appchargePrebuildEditor, AppchargeConfig appchargeConfig)
        {
            _path = path;
            _appchargePrebuildEditor = appchargePrebuildEditor;
            _appchargeConfig = appchargeConfig;
            _fileName = Path.GetFileName(_path);
        }

        public virtual void Backup()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            _backupPath = Path.Combine(projectRoot, "Appcharge", _fileName + _backupPathExtension);

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

        public virtual void Restore()
        {
            if (_backupPath != null && File.Exists(_backupPath))
            {
                File.Copy(_backupPath, _path, overwrite: true);
                File.Delete(_backupPath);

                string metaFilePath = _backupPath + _metaPathExtension;
                if (File.Exists(metaFilePath))
                    File.Delete(metaFilePath);

                Debug.Log("Restored original " + _fileName + " file.");
            }
        }

        public abstract void Update();
    }
}