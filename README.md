# Appcharge Payment Links SDK (Unity)
A lightweight Unity SDK for integrating **Appcharge Payment Links** into your game.
Use it to open a secure checkout and handle purchase callbacks with minimal setup.

**Supported platforms:** Android, iOS, WebGL, and Unity Editor (simulation).

---
## Features
- :link: Open Appcharge checkout directly from your Unity game
- :credit_card: Receive purchase success, failure, and cancellation callbacks
- :jigsaw: Easy integration using `ICheckoutPurchase`
- :gear: Automatic platform integration via `AppchargeConfig` (manifest, Gradle, iOS entitlements, and more)
- :package: Distributed as a Unity Package Manager (UPM) package

---
## Installation (UPM via Git URL)
1. Open **Unity**
2. Go to **Window → Package Manager**
3. Click the **+** button → **Add package from git URL…**
4. Enter your Git URL
5. Click **Add**

Import the included sample from the Package Manager if you want a ready-made integration reference.

---
## Configuration
Create an `AppchargeConfig` asset via **Appcharge → Configuration → AppchargeConfig** and place it under `Resources/Appcharge/`.

Key settings:
- **Checkout Public Key** and **Environment** — required for `Init(this)`
- **Browser Mode** — `Internal` (in-app) or `External` (system browser)
- **Enable Integration Options** — automatic Android/iOS/WebGL build-time setup
- **Enable Debug Mode** — prints integration changes to the Unity console; details are also written to `Logs/Appcharge/AppchargeIntegrationLogs.log`

---
## Basic Usage
### 1. Import Required Namespaces
```c#
using Appcharge.PaymentLinks;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;
```

### 2. Implement `ICheckoutPurchase`
Create a MonoBehaviour that receives callbacks from the SDK:
```c#
public class CheckoutSample : MonoBehaviour, ICheckoutPurchase
{
    public string CustomerId = "John Doe";

    public void Init()
    {
        PaymentLinksController.Instance.Init(this);
    }

    public void OnSessionSuccess(CheckoutResponse response)
    {
        PaymentLinksController.Instance.OpenCheckout(response.purchaseId, response.parsedUrl, CustomerId);
    }

    public void OnPurchaseSuccess(OrderResponseModel order)
    {
        Debug.Log($"Purchase Success: OrderId={order.orderId}, PaymentMethod={order.paymentMethodName}");
    }

    public void OnPurchaseCanceled(ErrorMessage error, OrderResponseModel order)
    {
        Debug.Log($"Purchase Canceled: OrderId={order?.orderId}");
    }

    public void OnPurchaseFailed(ErrorMessage error, OrderResponseModel order)
    {
        Debug.LogError($"Purchase Failed: Code={error.code}, Message={error.message}, OrderId={order?.orderId}");
    }

    public void OnInitialized()
    {
        Debug.Log("Payment Links SDK Initialized: " + PaymentLinksController.Instance.GetSdkVersion());
    }

    public void OnInitializeFailed(ErrorMessage error)
    {
        Debug.LogError($"Init Failed: Code={error.code}, Message={error.message}");
    }
}
```

---
## Typical Integration Flow
### **1. Initialize the SDK**
Uses credentials from `AppchargeConfig`:
```c#
PaymentLinksController.Instance.Init(this);
```

Or pass credentials explicitly:
```c#
PaymentLinksController.Instance.Init(checkoutPublicKey, "sandbox", this);
```

### **2. Create a checkout session (your backend)**
Your backend returns:
- `purchaseId`
- `parsedUrl`

### **3. Open checkout**
Pass the customer ID when opening checkout (not during init):
```c#
PaymentLinksController.Instance.OpenCheckout(purchaseId, parsedUrl, customerId);
```

### **4. Handle purchase callbacks**
- `OnPurchaseSuccess(OrderResponseModel order)`
- `OnPurchaseCanceled(ErrorMessage error, OrderResponseModel order)`
- `OnPurchaseFailed(ErrorMessage error, OrderResponseModel order)`

---
## SDK API Overview
### **PaymentLinksController**
- `Init(ICheckoutPurchase callback)`
- `Init(string checkoutToken, string environment, ICheckoutPurchase callback)`
- `OpenCheckout(string purchaseId, string parsedUrl, string customerId)`
- `GetSdkVersion()`
- `SetConfiguration(string property, object value)` — e.g. runtime `browserMode` or `enableMainThreadDispatcher`

### **Models**
- `CheckoutResponse`
- `OrderResponseModel`
- `ErrorMessage`
- `BrowserMode` — `Internal`, `External`

### **Interface**
- `ICheckoutPurchase`

---
## Migration from 2.x
- **`customerId` moved to `OpenCheckout`** — `Init` no longer accepts a customer ID
- **Legacy `OpenCheckout(url, sessionToken, purchaseId)` removed** — use `OpenCheckout(purchaseId, parsedUrl, customerId)`
- **Price points API removed** — `GetPricePoints()` and related callbacks are no longer available
- **Browser mode unified** — use `BrowserMode` in `AppchargeConfig` instead of separate iOS/Android browser settings

---
## Support
For help or integration questions:
Contact your Appcharge representative or open an issue in your repository.
