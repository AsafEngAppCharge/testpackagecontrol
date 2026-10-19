using Appcharge.AppDirect.Models;

namespace Appcharge.AppDirect.Interfaces
{
    public interface IAppDirectCallback
    {
        void OnInitialized();
        void OnUpdateCheckCompleted(UpdateInfo info);
        void OnDownloadStarted();
        void OnDownloadProgress(float progress);
        void OnDownloadCompleted();
        void OnInstallationSuccess();
        void OnUpdateInstallationFallback();
        void OnError(int errorCode);
    }
}
