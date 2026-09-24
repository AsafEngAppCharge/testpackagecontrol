# Appcharge Payment Links SDK (Unity)
A lightweight Unity SDK for integrating **Appcharge Payment Links** into your game.
Use it to open a secure checkout and handle purchase callbacks with minimal setup.
---
## Features
- :link: Open Appcharge checkout directly from your Unity game
- :credit_card: Receive purchase success / failure / cancel callbacks
- :jigsaw: Easy integration using `ICheckoutPurchase`
- :package: Distributed as a Unity Package Manager (UPM) package
---
## Installation (UPM via Git URL)
1. Open **Unity**
2. Go to **Window → Package Manager**
3. Click the **+** button → **Add package from git URL…**
4. Enter your Git URL
5. Click **Add**
---
## Configuration
Create an **AppchargeConfig** asset via **Appcharge → Configuration → AppchargeConfig** and place it at `Assets/Resources/Appcharge/AppchargeConfig.asset`.

Set:
- **Environment** (Sandbox, Staging, or Production)
- **Checkout Public Key** (from the Publisher Dashboard)
---
## Basic Usage
### 1. Import Required Namespaces
    using Appcharge.PaymentLinks;
    using Appcharge.PaymentLinks.Interfaces;
    using Appcharge.PaymentLinks.Models;
    using UnityEngine;
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
            Debug.Log($"Purchase Canceled: Code={error.code}, Message={error.message}, OrderId={order?.orderId}");
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
```c#
    PaymentLinksController.Instance.Init(this);
```
Or pass credentials explicitly:
```c#
    PaymentLinksController.Instance.Init(checkoutPublicKey, "sandbox", this);
```
Always call `Init` before `OpenCheckout`.
### **2. Create a checkout session (your backend)** 
Your backend returns the following:
- `purchaseId`
- `parsedUrl` (recommended), or `url`, `checkoutSessionToken`, and `purchaseId`
### **3. Open checkout**
Pass the customer ID when opening checkout (not during init):
```c#
    PaymentLinksController.Instance.OpenCheckout(purchaseId, parsedUrl, customerId);
```
### **4. Handle callbacks**
- `OnInitialized()`
- `OnInitializeFailed(ErrorMessage error)`
- `OnPurchaseSuccess(OrderResponseModel order)`
- `OnPurchaseCanceled(ErrorMessage error, OrderResponseModel order)`
- `OnPurchaseFailed(ErrorMessage error, OrderResponseModel order)`
---
## SDK API Overview
### **PaymentLinksController**
- `Init(ICheckoutPurchase callback)` — reads credentials from AppchargeConfig
- `Init(string checkoutToken, string environment, ICheckoutPurchase callback)`
- `OpenCheckout(string purchaseId, string parsedUrl, string customerId)`
- `GetSdkVersion()`
- `SetConfiguration(string property, object value)` — e.g. `"browserMode"`, `"debugMode"` (platform-specific)
### **Models**
- CheckoutResponse
- OrderResponseModel
- ErrorMessage
### **Interface**
- ICheckoutPurchase
---
## Support
For help or integration questions:
Contact your Appcharge representative or open an issue in your repository.
