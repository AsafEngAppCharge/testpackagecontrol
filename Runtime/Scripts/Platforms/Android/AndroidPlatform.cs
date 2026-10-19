using System;
using Appcharge.AppDirect.Config;
using Appcharge.AppDirect.Interfaces;
using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Platforms.Android
{
    public class AndroidPlatform : IAppDirectPlatform
    {
        private AndroidJavaObject _bridgeApi;
        private AndroidJavaObject _mainActivity;
        public IAppDirectCallback Callback { get; set; }
        private CallbackProxy _callbackProxy;

        private void EnsureInitialized()
        {
            if (_bridgeApi != null && _mainActivity != null)
                return;

            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                _mainActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _bridgeApi = new AndroidJavaObject("com.appcharge.appdirect.api.BridgeAPI");
            }
        }

        // Unity's player thread is not the Android main looper; ProcessLifecycleOwner.addObserver requires it.
        private void RunOnAndroidUiThread(Action action)
        {
            EnsureInitialized();
            if (_mainActivity == null || action == null)
            {
                Debug.LogError("Cannot run on Android UI thread.");
                return;
            }

            _mainActivity.Call("runOnUiThread", new AndroidJavaRunnable(action));
        }

        public void Initialize(string customerId, IAppDirectCallback callback)
        {
			AppDirectConfig editorConfig = ConfigUtility.GetConfig();
			if (editorConfig == null)
			{
				Debug.LogError("AppchargeConfig not found.");
				return;
			}

            Initialize(editorConfig.CheckoutPublicKey, customerId, editorConfig.Environment, callback);
        }

        public void Initialize(string checkoutPublicKey, string customerId, AppDirectEnvironment environment, IAppDirectCallback callback)
        {
            EnsureInitialized();

            if (_bridgeApi == null)
            {
                Debug.LogError("BridgeAPI is not initialized.");
                return;
            }

            Callback = callback;
            _callbackProxy ??= new CallbackProxy(this);

            RunOnAndroidUiThread(() =>
            {
                using (var configJavaObject = ConfigModelConverter.ToAndroidJavaObject(checkoutPublicKey, customerId, environment))
                {
                    _bridgeApi.Call("initialize", _mainActivity, configJavaObject, _callbackProxy);
                }
            });
        }

        public void CheckForUpdate()
        {
            EnsureInitialized();
            if (_bridgeApi == null)
            {
                Debug.LogError("BridgeAPI is not initialized.");
                return;
            }

            _bridgeApi.Call("checkForUpdate");
        }

        public void Download()
        {
            EnsureInitialized();
            if (_bridgeApi == null)
            {
                Debug.LogError("BridgeAPI is not initialized.");
                return;
            }

            _bridgeApi.Call("download");
        }

        public void Install()
        {
            EnsureInitialized();
            if (_bridgeApi == null)
            {
                Debug.LogError("BridgeAPI is not initialized.");
                return;
            }

            _bridgeApi.Call("install");
        }

        public void DownloadAndInstall()
        {
            EnsureInitialized();
            if (_bridgeApi == null)
            {
                Debug.LogError("BridgeAPI is not initialized.");
                return;
            }

            _bridgeApi.Call("downloadAndInstall");
        }

        public string GetSdkVersion()
        {
            EnsureInitialized();
            if (_bridgeApi == null)
            {
                Debug.LogError("BridgeAPI is not initialized.");
                return string.Empty;
            }

            return _bridgeApi.Call<string>("getSdkVersion");
        }

        public void OnAppFocusChanged(bool hasFocus)
        {
            EnsureInitialized();
            if (_bridgeApi == null)
            {
                Debug.LogError("BridgeAPI is not initialized.");
                return;
            }

            _bridgeApi.Call("onAppFocusChanged", hasFocus);
        }

        public void ConfigurePlatform(string property, object value)
        {
        }

        private class CallbackProxy : AndroidJavaProxy
        {
            private readonly AndroidPlatform _platform;

            public CallbackProxy(AndroidPlatform platform) : base("com.appcharge.appdirect.api.callback.AppDirectCallback")
            {
                _platform = platform;
            }

            public void onInitialized()
            {
                _platform.Callback?.OnInitialized();
            }

            public void onUpdateCheckCompleted(AndroidJavaObject info)
            {
                _platform.Callback?.OnUpdateCheckCompleted(UpdateInfoConverter.ToUpdateInfo(info));
            }

            public void onDownloadStarted()
            {
                _platform.Callback?.OnDownloadStarted();
            }

            public void onDownloadProgress(float progress)
            {
                _platform.Callback?.OnDownloadProgress(progress);
            }

            public void onDownloadCompleted()
            {
                _platform.Callback?.OnDownloadCompleted();
            }

            public void onInstallationSuccess()
            {
                _platform.Callback?.OnInstallationSuccess();
            }

            public void onUpdateInstallationFallback()
            {
                _platform.Callback?.OnUpdateInstallationFallback();
            }

            public void onError(int errorCode)
            {
                _platform.Callback?.OnError(errorCode);
            }
        }
    }
}
