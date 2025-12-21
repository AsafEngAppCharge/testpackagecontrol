using Appcharge.PaymentLinks;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;
using UnityEngine.UI;
using static SessionSample;

public class CheckoutSample : MonoBehaviour, ICheckoutPurchase
{
    public AppchargeEnvironment Environment;
    public string PublisherToken = "";
    public string CustomerId = "John Doe";
    [SerializeField] private Button _btnOpenCheckout;
    [SerializeField] private Button _btnGetPricePoints;
    [SerializeField] private Text _txtLogger;
    [SerializeField] private SessionSample _sessionSample;
    private string _orderId = "";

    public void Init()
    {
        _txtLogger.text = "Initializing Appcharge Checkout...";
        PaymentLinksController.Instance.Init(CustomerId, this);
    }

    public void OpenCheckout() 
    {
        _txtLogger.text = "Opening AC Checkout...";
        _sessionSample.OpenCheckout(Environment.ToString().ToLowerInvariant(), CustomerId,
            PublisherToken, OnSessionSuccess, OnSessionFailed);
        _btnOpenCheckout.interactable = false;
    }

    public void OnSessionSuccess(CheckoutResponse response)
    {
        PaymentLinksController.Instance.OpenCheckout(response.url, response.checkoutSessionToken, response.purchaseId);
    }
    public void OnSessionFailed(string error)
    {
        _txtLogger.text = "Checkout Session Failed: " + error;
        _btnOpenCheckout.interactable = true;
    }

    public void GetPricePoints()
    {
        PaymentLinksController.Instance.GetPricePoints();
    }

    public void OnPurchaseSuccess(OrderResponseModel order)
    {
        _txtLogger.text = string.Format("Purchase Success:\nOrderId: {0}\nPayment Method: {1}", order.orderId, order.paymentMethodName);
        _orderId = order.orderId;
        _btnOpenCheckout.interactable = true;
    }

    public void OnPurchaseFailed(ErrorMessage error)
    {
        _txtLogger.text = string.Format("Code: {0}\nMessage: {1}\nDetails: {2}", error.code, error.message, error.data);
        _btnOpenCheckout.interactable = true;
    }

    public void OnInitialized()
    {
        _btnOpenCheckout.interactable = true;
        _txtLogger.text = "SDK Initialized: " + PaymentLinksController.Instance.GetSdkVersion();
        _btnGetPricePoints.interactable = true;
    }

    public void OnInitializeFailed(ErrorMessage error)
    {
        _txtLogger.text = string.Format("Code: {0}\nMessage: {1}\nDetails: {2}", error.code, error.message, error.data);
    }

    public void OnPricePointsSuccess(PricePointsModel pricePoints)
    {
        _txtLogger.text = string.Format("Price Points Success: {0}", pricePoints.pricingPoints.Length);
    }

    public void OnPricePointsFail(ErrorMessage error)
    {
        _txtLogger.text = string.Format("Price Points Fail: {0}", error.message);
    }
}