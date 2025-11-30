# Appcharge Payment Links SDK (Unity)
A lightweight Unity SDK for integrating **Appcharge Payment Links** into your game.
Use it to open a secure checkout, handle purchase callbacks, and fetch price points with minimal setup.
---
## Features
- :link: Open Appcharge checkout directly from your Unity game
- :credit_card: Receive purchase success / failure callbacks
- :moneybag: Fetch available price points
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
        public AppchargeEnvironment Environment;
        public string CustomerId = "John Doe";
        // Initialize the SDK
        public void Init()
        {
            PaymentLinksController.Instance.Init(CustomerId, this);
        }
        // Backend returns checkout session → open checkout
        public void OnSessionSuccess(CheckoutResponse response)
        {
            PaymentLinksController.Instance.OpenCheckout(
                response.url,
                response.checkoutSessionToken,
                response.purchaseId
            );
        }
        // Fetch price points
        public void GetPricePoints()
        {
            PaymentLinksController.Instance.GetPricePoints();
        }
        // --- ICheckoutPurchase callbacks ---
        public void OnPurchaseSuccess(OrderResponseModel order)
        {
            Debug.Log($"Purchase Success: OrderId={order.orderId}, PaymentMethod={order.paymentMethodName}");
        }
        public void OnPurchaseFailed(ErrorMessage error)
        {
            Debug.LogError($"Purchase Failed: Code={error.code}, Message={error.message}, Details={error.data}");
        }
        public void OnInitialized()
        {
            Debug.Log("Payment Links SDK Initialized: " + PaymentLinksController.Instance.GetSdkVersion());
        }
        public void OnInitializeFailed(ErrorMessage error)
        {
            Debug.LogError($"Init Failed: Code={error.code}, Message={error.message}, Details={error.data}");
        }
        public void OnPricePointsSuccess(PricePointsModel pricePoints)
        {
            Debug.Log($"Price Points Success: {pricePoints.pricingPoints.Length} price points received");
        }
        public void OnPricePointsFail(ErrorMessage error)
        {
            Debug.LogError($"Price Points Fail: {error.message}");
        }
    }
```
---
## Typical Integration Flow
### **1. Initialize the SDK**
```c#
    PaymentLinksController.Instance.Init(CustomerId, this);
```
### **2. Create a checkout session (your backend)** 
Your backend returns the following:
- `url`
- `checkoutSessionToken`
- `purchaseId`
### **3. Open checkout**
```c#
    PaymentLinksController.Instance.OpenCheckout(url, checkoutSessionToken, purchaseId);
```
### **4. Handle purchase callbacks**
- `OnPurchaseSuccess(OrderResponseModel order)`
- `OnPurchaseFailed(ErrorMessage error)`
### **5. Fetch price points**
```C#
    PaymentLinksController.Instance.GetPricePoints();
```
Callbacks:
- `OnPricePointsSuccess(PricePointsModel pricePoints)`
- `OnPricePointsFail(ErrorMessage error)`
---
## SDK API Overview
### **PaymentLinksController**
- Init(string customerId, ICheckoutPurchase callback)
- OpenCheckout(string url, string checkoutSessionToken, string purchaseId)
- GetPricePoints()
- GetSdkVersion()
### **Models**
- CheckoutResponse
- OrderResponseModel
- PricePointsModel
- ErrorMessage
### **Interface**
- ICheckoutPurchase
---
## Support
For help or integration questions:
Contact your Appcharge representative or open an issue in your repository.