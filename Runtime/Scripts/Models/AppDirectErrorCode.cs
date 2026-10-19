namespace Appcharge.AppDirect.Models
{
    /// <summary>Int parity with native <c>SdkError</c>. Publishers receive the int via <c>OnError</c>.</summary>
    public enum AppDirectErrorCode
    {
        ServerError = 1000,
        GeneralServerError = 1001,
        TokenValidationError = 1007,

        InitializeInProgress = 1100,
        CheckForUpdateInProgress = 1101,
        DownloadInProgress = 1102,
        InstallInProgress = 1103,

        NotInitialized = 1200,

        BootInitializationError = 2000,
        BootEnvironmentError = 2001,
        BootTokenError = 2002,
        BootInProgress = 2005,
        BootConfigurationFile = 2006,

        CheckForUpdateError = 3000,
        NoAvailableVersion = 3001,
        UpdateVersionValidationFailed = 3002,
        UpdateCheckParseError = 3003,
        LowOrEqualVersionCode = 3004,

        DownloadError = 4000,
        DownloadFailed = 4001,
        DownloadWriteFailed = 4002,
        InsufficientStorage = 4003,
        DownloadUrlError = 4004,
        DownloadTimeout = 4005,
        DownloadUrlExpired = 4006,
        DownloadFileMismatch = 4007,
        DownloadFailedAfterRetries = 4008,

        InstallError = 5000,
        InstallCanceled = 5001,
        InstallPermissionRequired = 5002,
        InstallPendingIntentFailed = 5003,
        InstallIntentLaunchFailed = 5004,

        DownloadAndInstallError = 6000,

        FocusChangedError = 7000,
        IntegrationError = 7100,

        InvalidArgumentBootNotReady = 8000,
        InvalidArgumentCustomerId = 8002,

        NoInternetConnection = 9001,
        IOError = 9100,
        UnknownError = 9999
    }
}
