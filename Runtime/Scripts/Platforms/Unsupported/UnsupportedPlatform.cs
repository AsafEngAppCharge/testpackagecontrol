using Appcharge.AppDirect.Interfaces;
using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Platforms.Unsupported
{
    public sealed class UnsupportedPlatform : IAppDirectPlatform
    {
        public IAppDirectCallback Callback { get; set; }

        public void Initialize(string customerId, IAppDirectCallback callback)
        {
            Initialize(null, customerId, default, callback);
        }

        public void Initialize(string checkoutPublicKey, string customerId, AppDirectEnvironment environment, IAppDirectCallback callback)
        {
            Callback = callback;
            Debug.LogWarning("AppDirect is Android-only. Initialize is unavailable on this platform.");
            callback?.OnError((int)AppDirectErrorCode.UnknownError);
        }

        public void CheckForUpdate() => Unavailable("CheckForUpdate");
        public void Download() => Unavailable("Download");
        public void Install() => Unavailable("Install");
        public void DownloadAndInstall() => Unavailable("DownloadAndInstall");

        public string GetSdkVersion()
        {
            Debug.LogWarning("AppDirect is Android-only. GetSdkVersion is unavailable on this platform.");
            return "0.0.0";
        }

        public void OnAppFocusChanged(bool hasFocus)
        {
        }

        public void ConfigurePlatform(string property, object value)
        {
        }

        private void Unavailable(string method)
        {
            Debug.LogWarning("AppDirect is Android-only. " + method + " is unavailable on this platform.");
            Callback?.OnError((int)AppDirectErrorCode.UnknownError);
        }
    }
}
