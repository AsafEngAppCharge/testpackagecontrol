using Appcharge.PaymentLinks;
using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;
using UnityEngine.UI;

public class CheckoutSample : MonoBehaviour, ICheckoutPurchase
{
    [SerializeField] private GameObject _initPopup;
    [SerializeField] private Button _btnOpenCheckout;
    [SerializeField] private Button _btnGetPricePoints;
    [SerializeField] private Text _environmentText;
    [SerializeField] private Text _loggerText;
    [SerializeField] private InputField _inputCustomerId;
    private string _customerId = "John Doe";
    private AppchargeConfig _config;

    private void Start()
    {
        ValidateSerializedReferences();
        _config = ConfigUtility.GetConfig();
        if (_config == null)
        {
            LogMessage("Error: AppchargeConfig not found. Create one via Appcharge > Configuration > AppchargeConfig.");
            return;
        }

        _environmentText.text = _config.Environment.ToString();
    }

    public void Initialize()
    {
        if (!Validation())
            return;

        _initPopup.SetActive(false);
        PaymentLinksController.Instance.Init(_customerId, this);
        LogMessage("Waiting for SDK to initialize.");
    }

    /// <summary>
    /// To open the checkout, you must obtain purchaseId and parsedUrl from your server.
    /// Do not modify the arguments. The SDK will handle the rest.
    /// (Create Checkout Session API: https://docs.appcharge.com/api-reference/checkout/checkout-session/create-checkout-session)
    /// Using the response from the API, call PaymentLinksController.Instance.OpenCheckout(purchaseId, parsedUrl);
    /// </summary>
    public void OpenCheckout()
    {
        string purchaseId = "";
        string parsedUrl = "";

        if (string.IsNullOrWhiteSpace(purchaseId) || string.IsNullOrWhiteSpace(parsedUrl))
        {
            LogMessage("Error: Purchase ID or parsed URL is empty. Please provide valid purchase ID and parsed URL from your server.");
            return;
        }

        PaymentLinksController.Instance.OpenCheckout(purchaseId, parsedUrl);
    }

    public void GetPricePoints()
    {
        if (PaymentLinksController.Instance == null)
        {
            LogMessage("Error: PaymentLinksController not ready. Initialize the SDK first.");
            return;
        }

        PaymentLinksController.Instance.GetPricePoints();
    }

    private bool Validation()
    {
        if (_inputCustomerId != null && !string.IsNullOrEmpty(_inputCustomerId.text))
            _customerId = _inputCustomerId.text.Trim();

        if (string.IsNullOrWhiteSpace(_customerId))
        {
            LogMessage("Error: Customer ID is empty. Please enter a customer ID before initializing.");
            return false;
        }
        if (_config == null)
        {
            LogMessage("Error: AppchargeConfig not found. Please create one via Appcharge > Configuration > AppchargeConfig.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_config.CheckoutPublicKey))
        {
            LogMessage("Error: Checkout Public Key is empty. Please set it in AppchargeConfig before initializing.");
            return false;
        }
        return true;
    }

    public void OnPurchaseSuccess(OrderResponseModel order)
    {
        _btnOpenCheckout.interactable = true;
        LogMessage(string.Format("Purchase Success:\nOrderId: {0}\nPayment Method: {1}", order?.orderId, order?.paymentMethodName));
    }

    public void OnPurchaseFailed(ErrorMessage error, OrderResponseModel order)
    {
        _btnOpenCheckout.interactable = true;
        LogMessage(string.Format("Code: {0}\nMessage: {1}\nDetails: {2}", error?.code, error?.message, order?.orderId));
    }

    public void OnInitialized()
    {
        _btnOpenCheckout.interactable = true;
        _btnGetPricePoints.interactable = true;
        LogMessage("SDK Initialized: " + (PaymentLinksController.Instance != null ? PaymentLinksController.Instance.GetSdkVersion() : "?"));
    }

    public void OnInitializeFailed(ErrorMessage error)
    {
        LogMessage(string.Format("Code: {0}\nMessage: {1}", error?.code, error?.message));
    }

    public void OnPricePointsSuccess(PricePointsModel pricePoints)
    {
        LogMessage("Price Points Success: " + (pricePoints?.pricingPoints?.Length ?? 0));
    }

    public void OnPricePointsFail(ErrorMessage error)
    {
        LogMessage(string.Format("Price Points Fail: {0}", error?.message ?? "unknown"));
    }

    public void ShowInitializationPopup(bool show)
    {
        _initPopup.SetActive(show);
    }
    private void LogMessage(string message, bool hidePopup = false)
    {
        _loggerText.text = message;
        Debug.Log(message);
        
        if (hidePopup)
            ShowInitializationPopup(false);
    }

    private void ValidateSerializedReferences()
    {
        if (_initPopup == null) Debug.LogWarning("[CheckoutSample] _initPopup is not assigned.");
        if (_btnOpenCheckout == null) Debug.LogWarning("[CheckoutSample] _btnOpenCheckout is not assigned.");
        if (_btnGetPricePoints == null) Debug.LogWarning("[CheckoutSample] _btnGetPricePoints is not assigned.");
        if (_environmentText == null) Debug.LogWarning("[CheckoutSample] _environmentText is not assigned.");
        if (_loggerText == null) Debug.LogWarning("[CheckoutSample] _loggerText is not assigned.");
    }

}