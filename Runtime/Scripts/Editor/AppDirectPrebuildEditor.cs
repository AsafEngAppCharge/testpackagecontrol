using System;
using System.IO;
using Appcharge.AppDirect.Config;
#if UNITY_EDITOR
using UnityEditor.Build;
#endif
using UnityEngine;

namespace Appcharge.AppDirect.Editor
{
    public sealed class AppDirectPrebuildEditor
    {
        private readonly string _gradleTemplatePath = Path.Combine(Application.dataPath, "Plugins/Android/gradleTemplate.properties");
        private readonly string _mainTemplatePath = Path.Combine(Application.dataPath, "Plugins/Android/mainTemplate.gradle");
        private readonly string _manifestPath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

        private Prebuilder[] _steps;
        private AppDirectConfig _config;
        private PrebuildLogger _logger;

        public void OnPreprocessBuild()
        {
            _config = ConfigUtility.GetConfig();
            _logger = new PrebuildLogger();
            _steps = new Prebuilder[]
            {
                new SyncSdkVersionPrebuild(this, _config),
                new GradleTemplatePrebuild(_gradleTemplatePath, this, _config),
                new MainTemplatePrebuild(_mainTemplatePath, this, _config),
                new AndroidManifestPrebuild(_manifestPath, this, _config)
            };

            if (!_config.EnableIntegrationOptions)
                return;

            try
            {
                _logger.ClearLogs();
                foreach (var step in _steps)
                    step.Update();

                if (_config.EnableDebugMode)
                    _logger.PrintLog();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                throw new BuildFailedException("AppDirect prebuild failed: " + ex.Message);
#else
                Debug.LogError("AppDirect prebuild failed: " + ex.Message);
#endif
            }
        }

        public void LogToFile(string message, bool isError = false)
        {
            _logger.Log(message, isError);
        }
    }
}
