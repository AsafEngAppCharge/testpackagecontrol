#if UNITY_EDITOR
namespace Appcharge.PaymentLinks.Platforms.Editor.Models
{
    public static class EditorErrorCodes
    {
        public const int GeneralServerError = 1001;
        public const string GeneralServerErrorName = "GENERAL_SERVER_ERROR";

        public const int BootInitializationError = 2000;
        public const string BootInitializationErrorName = "BOOT_INITIALIZATION_ERROR";

        public const int BootInitAlreadyInProgress = 2005;
        public const string BootInitAlreadyInProgressName = "BOOT_INIT_ALREADY_IN_PROGRESS";
        public const string BootInitAlreadyInProgressMessage = "SDK initialization is already in progress.";

        public const int BrowserClosed = 3000;
        public const string BrowserClosedName = "BROWSER_CLOSED";
        public const string BrowserClosedMessage = "Browser closed before payment status was confirmed.";

        public const int ChargeCanceled = 3002;
        public const string ChargeCanceledName = "CHARGE_CANCELED";
        public const string OrderValidationCanceledMessage = "Purchase canceled because the checkout was closed.";

        public const int ChargeFailed = 3003;
        public const string ChargeFailedName = "CHARGE_FAILED";

        public const int ValidateOrderTimeout = 3004;
        public const string ValidateOrderTimeoutName = "VALIDATE_ORDER_TIMEOUT";
        public const string ValidateOrderTimeoutMessage = "Order validation timed out while awaiting publisher grant callback.";

        public const int PurchaseNotFound = 3005;
        public const string PurchaseNotFoundName = "PURCHASE_NOT_FOUND";
        public const string PurchaseNotFoundMessage = "Order not found — checkout closed before order creation.";

        public const int InvalidArgumentPurchaseId = 8000;
        public const string InvalidArgumentPurchaseIdName = "INVALID_ARGUMENT_PURCHASE_ID";
        public const string InvalidArgumentPurchaseIdMessage = "Purchase ID is invalid. Provided Purchase ID: {0}";

        public const int InvalidArgumentParsedUrl = 8001;
        public const string InvalidArgumentParsedUrlName = "INVALID_ARGUMENT_PARSED_URL";
        public const string InvalidArgumentParsedUrlMessage = "Parsed URL argument is missing or invalid. Verify the URL passed to openCheckout.";

        public const int InvalidArgumentCustomerId = 8002;
        public const string InvalidArgumentCustomerIdName = "EMPTY_CUSTOMER_ID";
        public const string InvalidArgumentCustomerIdMessage = "Customer ID is missing. Provide a valid Customer ID in openCheckout().";

        public const int UnknownError = 9999;
        public const string UnknownErrorName = "UNKNOWN_ERROR";
    }
}
#endif
