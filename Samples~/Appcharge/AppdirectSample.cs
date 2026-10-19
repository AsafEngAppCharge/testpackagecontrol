using Appcharge.AppDirect;
using Appcharge.AppDirect.Config;
using Appcharge.AppDirect.Interfaces;
using Appcharge.AppDirect.Models;
using UnityEngine;
using UnityEngine.UI;

public class AppdirectSample : MonoBehaviour, IAppDirectCallback
{
    [SerializeField] private Button _btnCheckForUpdate;
    [SerializeField] private Button _btnDownload;
    [SerializeField] private Button _btnInstall;
    [SerializeField] private Button _btnDownloadAndInstall;
    [SerializeField] private Button _btnSandbox;
    [SerializeField] private Button _btnProduction;
    [SerializeField] private Button _btnStaging;
    [SerializeField] private Text _loggerText;
    [SerializeField] private InputField _inputCustomerId;

    private static readonly Color SelectedEnvColor = new Color(0.35f, 0.85f, 0.45f, 1f);
    private static readonly Color UnselectedEnvColor = Color.white;

    private AppDirectConfig _config;
    private AppDirectEnvironment _environment = AppDirectEnvironment.Staging;

    private void Start()
    {
        ValidateSerializedReferences();
        SetActionButtonsInteractable(false);
        WireEnvironmentButtons();

        try
        {
            _config = ConfigUtility.GetConfig();
        }
        catch (System.Exception ex)
        {
            LogMessage("Error: " + ex.Message);
            return;
        }

        SetEnvironment(_config.Environment);
    }

    public void Initialize()
    {
        if (!Validation())
            return;

        AppDirectController.Instance.Initialize(
            _config.CheckoutPublicKey,
            "ShayTest",
            AppDirectEnvironment.Staging,
            this);
        LogMessage("Waiting for SDK to initialize (" + _environment + ").");
    }

    public void SetEnvironment(AppDirectEnvironment environment)
    {
        _environment = environment;
        RefreshEnvironmentHighlight();
        LogMessage("Environment: " + _environment);
    }

    private void RefreshEnvironmentHighlight()
    {
        SetEnvButtonColor(_btnStaging, _environment == AppDirectEnvironment.Staging);
        SetEnvButtonColor(_btnSandbox, _environment == AppDirectEnvironment.Sandbox);
        SetEnvButtonColor(_btnProduction, _environment == AppDirectEnvironment.Production);
    }

    private static void SetEnvButtonColor(Button button, bool selected)
    {
        if (button == null)
            return;

        Graphic image = button.targetGraphic;
        if (image == null)
            image = button.GetComponent<Graphic>();

        image.color = selected ? SelectedEnvColor : UnselectedEnvColor;
    }

    public void CheckForUpdate()
    {
        AppDirectController.Instance.CheckForUpdate();
        LogMessage("Checking for update…");
    }

    public void Download()
    {
        AppDirectController.Instance.Download();
    }

    public void Install()
    {
        AppDirectController.Instance.Install();
        LogMessage("Install requested…");
    }

    public void DownloadAndInstall()
    {
        AppDirectController.Instance.DownloadAndInstall();
    }

    public void OnInitialized()
    {
        SetActionButtonsInteractable(true);
        string version = AppDirectController.Instance != null
            ? AppDirectController.Instance.GetSdkVersion()
            : "?";
        LogMessage("SDK Initialized: " + version + " (" + _environment + ")");
    }

    public void OnUpdateCheckCompleted(UpdateInfo info)
    {
        if (info != null && info.HasUpdate)
            LogMessage("Update available (" + info.UpdateState + ")");
        else
            LogMessage("No update");
    }

    public void OnDownloadStarted()
    {
        LogMessage("Download started");
    }

    public void OnDownloadProgress(float progress)
    {
        LogMessage("Downloading… " + Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f) + "%");
    }

    public void OnDownloadCompleted()
    {
        LogMessage("Download completed");
    }

    public void OnInstallationSuccess()
    {
        LogMessage("Installation success");
    }

    public void OnUpdateInstallationFallback()
    {
        LogMessage("Installation fallback");
    }

    public void OnError(int errorCode)
    {
        LogMessage("Error " + errorCode);
    }

    private void WireEnvironmentButtons()
    {
        if (_btnStaging != null)
        {
            _btnStaging.onClick.RemoveAllListeners();
            _btnStaging.onClick.AddListener(() => SetEnvironment(AppDirectEnvironment.Staging));
        }

        if (_btnSandbox != null)
        {
            _btnSandbox.onClick.RemoveAllListeners();
            _btnSandbox.onClick.AddListener(() => SetEnvironment(AppDirectEnvironment.Sandbox));
        }

        if (_btnProduction != null)
        {
            _btnProduction.onClick.RemoveAllListeners();
            _btnProduction.onClick.AddListener(() => SetEnvironment(AppDirectEnvironment.Production));
        }
    }

    private bool Validation()
    {
        if (_config == null)
        {
            LogMessage("Error: AppDirectConfig not found. Create one via Appcharge > AppDirect > Configuration.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_config.CheckoutPublicKey))
        {
            LogMessage("Error: Checkout Public Key is empty. Set it in AppDirectConfig before initializing.");
            return false;
        }

        return true;
    }

    private void SetActionButtonsInteractable(bool interactable)
    {
        if (_btnCheckForUpdate != null) _btnCheckForUpdate.interactable = interactable;
        if (_btnDownload != null) _btnDownload.interactable = interactable;
        if (_btnInstall != null) _btnInstall.interactable = interactable;
        if (_btnDownloadAndInstall != null) _btnDownloadAndInstall.interactable = interactable;
    }

    private void LogMessage(string message)
    {
        if (_loggerText != null)
            _loggerText.text = message;
        Debug.Log(message);
    }

    private void ValidateSerializedReferences()
    {
        if (_btnCheckForUpdate == null) Debug.LogWarning("[AppdirectSample] _btnCheckForUpdate is not assigned.");
        if (_btnDownload == null) Debug.LogWarning("[AppdirectSample] _btnDownload is not assigned.");
        if (_btnInstall == null) Debug.LogWarning("[AppdirectSample] _btnInstall is not assigned.");
        if (_btnDownloadAndInstall == null) Debug.LogWarning("[AppdirectSample] _btnDownloadAndInstall is not assigned.");
        if (_btnSandbox == null) Debug.LogWarning("[AppdirectSample] _btnSandbox is not assigned.");
        if (_btnProduction == null) Debug.LogWarning("[AppdirectSample] _btnProduction is not assigned.");
        if (_btnStaging == null) Debug.LogWarning("[AppdirectSample] _btnStaging is not assigned.");
        if (_loggerText == null) Debug.LogWarning("[AppdirectSample] _loggerText is not assigned.");
        if (_inputCustomerId == null) Debug.LogWarning("[AppdirectSample] _inputCustomerId is not assigned.");
    }
}
