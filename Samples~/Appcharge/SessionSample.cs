using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SessionSample: MonoBehaviour
{
    private int _currentPrice = 399;
    private string _currentCurrency = "usd";
    
    public void OpenCheckout(string environment, string customerId, string publisherToken, Action<CheckoutResponse> onSuccess, Action<string> onFailed) {
        StartCoroutine(SendSessionRequest(environment, customerId, publisherToken,
         onSuccess, onFailed));
    }

    private IEnumerator SendSessionRequest(string environment, string customerId, string publisherToken, Action<CheckoutResponse> onSuccess, Action<string> onFailed)
    {
        string sessionUrl = "";

        switch(environment) {
            case "sandbox":
                sessionUrl = "SESSION_URL_SANDBOX";
                break;
            case "production":
                sessionUrl = "SESSION_URL_PRODUCTION";
                break;
        }

        string jsonBody = @"{
            ""customer"": {
                ""id"": """ + customerId + @""",
                ""email"": ""test@test.com""
            },
            ""priceDetails"": {
                ""price"": """ + _currentPrice + @""",
                ""currency"": """ + _currentCurrency + @"""
            },
            ""offer"": {
                ""name"": ""Coins Shop"",
                ""sku"": ""CoinsShop"",
                ""assetUrl"": """",
                ""description"": ""Coin Pack Bundle""
            }
        }";

        using (var request = new UnityWebRequest(sessionUrl, UnityWebRequest.kHttpVerbPOST))
        {
            var body = System.Text.Encoding.UTF8.GetBytes(jsonBody);

            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("x-publisher-token", publisherToken);

            yield return request.SendWebRequest();

            var status = request.responseCode;
            var text = request.downloadHandler?.text;

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<CheckoutResponse>(text);
                onSuccess?.Invoke(response);
            }
            else
            {
                Debug.LogError($"Session request failed ({status}): {request.error}\nBody: {text}");
                onFailed?.Invoke(request.error);
            }
        }
    }

    [Serializable]
    public class CheckoutResponse
    {
        public string checkoutSessionToken;
        public string purchaseId;
        public string url;
    }
}