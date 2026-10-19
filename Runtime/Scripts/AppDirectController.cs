using System;
using Appcharge.AppDirect.Config;
using Appcharge.AppDirect.Interfaces;
using Appcharge.AppDirect.Models;
using Appcharge.AppDirect.Platforms.Android;
using Appcharge.AppDirect.Platforms.Unsupported;
using Appcharge.AppDirect.Threading;
using UnityEngine;

namespace Appcharge.AppDirect
{
    public sealed class AppDirectController
    {
        private static AppDirectController _instance;
        private static IAppDirectPlatform _currentPlatform;

        private static IAppDirectPlatform Platform
        {
            get
            {
                if (_currentPlatform != null)
                    return _currentPlatform;

                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        _currentPlatform = new AndroidPlatform();
                        break;
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.LinuxEditor:
                        _currentPlatform = CreateEditorPlatform();
                        break;
                    default:
                        _currentPlatform = new UnsupportedPlatform();
                        break;
                }

                return _currentPlatform;
            }
        }

        private AppDirectController()
        {
        }

        public static AppDirectController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AppDirectController();
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void WarmSingletonAfterFirstSceneLoad()
        {
            _ = Instance;
        }

        public void Initialize(string customerId, IAppDirectCallback callback)
        {
            DispatchOnMainThread(InitializeOnMainThread);

            void InitializeOnMainThread()
            {
                ApplyMainThreadDispatcherFromConfig();
                EnsureFocusForwarder();
                Platform.Initialize(customerId, WrapPublisherCallback(callback));
            }
        }

        public void Initialize(string checkoutPublicKey, string customerId, AppDirectEnvironment environment, IAppDirectCallback callback)
        {
            Debug.Log("Initialize: " + checkoutPublicKey + " " + customerId + " " + environment + " " + callback);
            DispatchOnMainThread(InitializeOnMainThread);

            void InitializeOnMainThread()
            {
                Debug.Log("InitializeOnMainThread");
                ApplyMainThreadDispatcherFromConfig();
                EnsureFocusForwarder();
                Platform.Initialize(checkoutPublicKey, customerId, environment, WrapPublisherCallback(callback));
            }
        }

        public void CheckForUpdate()
        {
            DispatchOnMainThread(CheckForUpdateOnMainThread);

            void CheckForUpdateOnMainThread()
            {
                Platform.CheckForUpdate();
            }
        }

        public void Download()
        {
            DispatchOnMainThread(DownloadOnMainThread);

            void DownloadOnMainThread()
            {
                Platform.Download();
            }
        }

        public void Install()
        {
            DispatchOnMainThread(InstallOnMainThread);

            void InstallOnMainThread()
            {
                Platform.Install();
            }
        }

        public void DownloadAndInstall()
        {
            DispatchOnMainThread(DownloadAndInstallOnMainThread);

            void DownloadAndInstallOnMainThread()
            {
                Platform.DownloadAndInstall();
            }
        }

        public string GetSdkVersion()
        {
            return DispatchOnMainThread(GetSdkVersionOnMainThread);

            string GetSdkVersionOnMainThread()
            {
                return Platform.GetSdkVersion();
            }
        }

        public void OnAppFocusChanged(bool hasFocus)
        {
            // DispatchOnMainThread(OnAppFocusChangedOnMainThread);
            //
            // void OnAppFocusChangedOnMainThread()
            // {
            //     Platform.OnAppFocusChanged(hasFocus);
            // }
        }

        public void SetConfiguration(string property, object value)
        {
            DispatchOnMainThread(SetConfigurationOnMainThread);

            void SetConfigurationOnMainThread()
            {
                if (property.Equals("enableMainThreadDispatcher", StringComparison.OrdinalIgnoreCase) && value is bool enabled)
                {
                    MainThreadDispatcher.Enabled = enabled;
                    return;
                }

                Platform.ConfigurePlatform(property, value);
            }
        }

        private static void DispatchOnMainThread(Action action)
        {
            MainThreadDispatcher.RunSync(action);
        }

        private static T DispatchOnMainThread<T>(Func<T> func)
        {
            return MainThreadDispatcher.RunSync(func);
        }

        private static void EnsureFocusForwarder()
        {
            if (Application.platform == RuntimePlatform.Android)
                AppDirectFocusForwarder.EnsureExists();
        }

        private static IAppDirectCallback WrapPublisherCallback(IAppDirectCallback callback)
        {
            Debug.Log("WrapPublisherCallback: " + callback);
            if (callback == null)
                return null;

            return new MainThreadAppDirectCallback(callback);
        }

        private static void ApplyMainThreadDispatcherFromConfig()
        {
            try
            {
                var config = ConfigUtility.GetConfig();
                MainThreadDispatcher.Enabled = config.EnableMainThreadDispatcher;
                MainThreadDispatcher.DebugLogging = config.EnableDebugMode;
            }
            catch (Exception)
            {
                MainThreadDispatcher.Enabled = true;
                MainThreadDispatcher.DebugLogging = false;
            }
        }

        private static IAppDirectPlatform CreateEditorPlatform()
        {
            var editorPlatformType = Type.GetType("Appcharge.AppDirect.Platforms.Editor.EditorPlatform, Appcharge.AppDirect.Platforms.Editor");
            if (editorPlatformType != null)
            {
                try
                {
                    return Activator.CreateInstance(editorPlatformType) as IAppDirectPlatform;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to create Editor platform: " + ex.Message);
                }
            }
            else
            {
                Debug.LogWarning("Could not find EditorPlatform. Falling back to UnsupportedPlatform.");
            }

            return new UnsupportedPlatform();
        }
    }
}
