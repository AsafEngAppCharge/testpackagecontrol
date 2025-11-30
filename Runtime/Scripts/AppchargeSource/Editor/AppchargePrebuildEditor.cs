using System.IO;
using Appcharge.PaymentLinks.Config;
using UnityEngine;

namespace Appcharge.PaymentLinks.Editor
{
    public class AppchargePrebuildEditor
    {
        #region Initialize
        private void Init()
        {
            _appchargeConfig = ConfigUtility.GetConfig();
            _logger = new PrebuildLogger();

            if (RuntimePlatform.Android != Application.platform)
            {
                _configLogic = new Prebuilder[3];
                _configLogic[0] = new GradleTemplatePrebuild(gradleTemplatePath, this, _appchargeConfig);
                _configLogic[1] = new MainTemplatePrebuild(mainTemplatePath, this, _appchargeConfig);
                _configLogic[2] = new AndroidManifestPrebuild(manifestPath, this, _appchargeConfig);
            }
        }
        #endregion
        #region Variables
        public int callbackOrder => 0;
        private Prebuilder[] _configLogic;
        private readonly string gradleTemplatePath = Path.Combine(Application.dataPath, "Plugins/Android/gradleTemplate.properties");
        private readonly string mainTemplatePath = Path.Combine(Application.dataPath, "Plugins/Android/mainTemplate.gradle");
        private readonly string manifestPath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
        private AppchargeConfig _appchargeConfig;
        private PrebuildLogger _logger;
        #endregion
        #region Reporters
        public void OnPreprocessBuild()
        {
            Init();

            if (!_appchargeConfig.EnableIntegrationOptions)
                return;

            try
            {
                _logger.ClearLogs();
                UpdateFiles();
                if (_appchargeConfig.EnableDebugMode)
                    _logger.PrintLog();
            }
            catch
            {
            }
        }

        #endregion
        #region Backup and Restore
        private void UpdateFiles()
        {
            if (_configLogic == null)
                return;

            foreach (Prebuilder configLogic in _configLogic)
            {
                configLogic.Update();
            }
        }

        public void LogToFile(string message, bool isError = false)
        {
            _logger.Log(message, isError);
        }

        #endregion
    }
}