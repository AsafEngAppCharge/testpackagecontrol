using Appcharge.AppDirect.Models;

namespace Appcharge.AppDirect.Interfaces
{
    public interface IAppDirectPlatform
    {
        IAppDirectCallback Callback { get; set; }

        void Initialize(string customerId, IAppDirectCallback callback);
        void Initialize(string checkoutPublicKey, string customerId, AppDirectEnvironment environment, IAppDirectCallback callback);
        void CheckForUpdate();
        void Download();
        void Install();
        void DownloadAndInstall();
        string GetSdkVersion();
        void OnAppFocusChanged(bool hasFocus);
        void ConfigurePlatform(string property, object value);
    }
}
