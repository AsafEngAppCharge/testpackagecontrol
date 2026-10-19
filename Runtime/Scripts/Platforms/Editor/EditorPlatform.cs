using Appcharge.AppDirect.Config;
using Appcharge.AppDirect.Interfaces;
using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Platforms.Editor
{
    /// <summary>
    /// In-editor stub. Simulates check/download and records a pending install.
    /// OnInstallationSuccess fires on the next Initialize, matching native behavior.
    /// </summary>
    public sealed class EditorPlatform : IAppDirectPlatform
    {
        private bool _pendingInstallSuccess;
        private bool _simulateHasUpdate = true;
        private UpdateState _simulateUpdateState = UpdateState.Soft;

        public IAppDirectCallback Callback { get; set; }

        public void Initialize(string customerId, IAppDirectCallback callback)
        {
            AppDirectConfig config;
            try
            {
                config = ConfigUtility.GetConfig();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
                return;
            }

            Initialize(config.CheckoutPublicKey, customerId, config.Environment, callback);
        }

        public void Initialize(string checkoutPublicKey, string customerId, AppDirectEnvironment environment, IAppDirectCallback callback)
        {
            Callback = callback;
            callback?.OnInitialized();
            if (_pendingInstallSuccess)
            {
                _pendingInstallSuccess = false;
                callback?.OnInstallationSuccess();
            }
        }

        public void CheckForUpdate()
        {
            if (Callback == null)
                return;

            var info = new UpdateInfo
            {
                Current = new VersionData
                {
                    PackageName = Application.identifier,
                    VersionCode = 1,
                    VersionName = "1.0.0"
                },
                UpdateState = _simulateHasUpdate ? _simulateUpdateState : UpdateState.None,
                Changelog = _simulateHasUpdate ? "Editor simulated update" : null,
                FileSizeBytes = _simulateHasUpdate ? 1024 : (long?)null,
                New = _simulateHasUpdate
                    ? new VersionData
                    {
                        PackageName = Application.identifier,
                        VersionCode = 2,
                        VersionName = "1.1.0"
                    }
                    : null
            };
            Callback.OnUpdateCheckCompleted(info);
        }

        public void Download()
        {
            Callback?.OnDownloadStarted();
            Callback?.OnDownloadProgress(1f);
            Callback?.OnDownloadCompleted();
        }

        public void Install()
        {
            _pendingInstallSuccess = true;
            Debug.Log("AppDirect editor stub: system installer is not available. OnInstallationSuccess fires on the next Initialize.");
        }

        public void DownloadAndInstall()
        {
            Download();
            _pendingInstallSuccess = true;
            Debug.Log("AppDirect editor stub: download finished. OnInstallationSuccess fires on the next Initialize.");
        }

        public string GetSdkVersion()
        {
            return SdkVersion.UnitySdkVersion;
        }

        public void OnAppFocusChanged(bool hasFocus)
        {
        }

        public void ConfigurePlatform(string property, object value)
        {
            if (property == "simulateHasUpdate" && value is bool hasUpdate)
                _simulateHasUpdate = hasUpdate;

            if (property == "simulateUpdateState" && value is UpdateState state)
                _simulateUpdateState = state;
        }
    }
}
