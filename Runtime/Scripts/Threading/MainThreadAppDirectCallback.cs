using Appcharge.AppDirect.Interfaces;
using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Threading
{
    public sealed class MainThreadAppDirectCallback : IAppDirectCallback
    {
        private readonly IAppDirectCallback _inner;

        public MainThreadAppDirectCallback(IAppDirectCallback inner)
        {
            _inner = inner;
        }

        public void OnInitialized()
        {
            Debug.Log("MainThreadAppDirectCallback.OnInitialized");
            MainThreadDispatcher.Run(() => _inner?.OnInitialized());
        }

        public void OnUpdateCheckCompleted(UpdateInfo info)
        {
            MainThreadDispatcher.Run(() => _inner?.OnUpdateCheckCompleted(info));
        }

        public void OnDownloadStarted()
        {
            MainThreadDispatcher.Run(() => _inner?.OnDownloadStarted());
        }

        public void OnDownloadProgress(float progress)
        {
            MainThreadDispatcher.Run(() => _inner?.OnDownloadProgress(progress));
        }

        public void OnDownloadCompleted()
        {
            MainThreadDispatcher.Run(() => _inner?.OnDownloadCompleted());
        }

        public void OnInstallationSuccess()
        {
            MainThreadDispatcher.Run(() => _inner?.OnInstallationSuccess());
        }

        public void OnUpdateInstallationFallback()
        {
            MainThreadDispatcher.Run(() => _inner?.OnUpdateInstallationFallback());
        }

        public void OnError(int errorCode)
        {
            MainThreadDispatcher.Run(() => _inner?.OnError(errorCode));
        }
    }
}
