using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android
{
	public class AndroidPlatform : ICheckoutPlatform
	{
		private AndroidJavaObject _bridgeApi;
		private AndroidJavaObject _mainActivity;

		public ICheckoutPurchase Callback { get; set; }

		private void EnsureInitialized()
		{
			if (_bridgeApi != null && _mainActivity != null) return;

			using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				_mainActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				_bridgeApi = new AndroidJavaObject("com.appcharge.core.BridgeAPI");
			}
		}

		public void Init(string customerId, ICheckoutPurchase callback)
		{
			AppchargeConfig editorConfig = ConfigUtility.GetConfig();
			if (editorConfig == null)
			{
				Debug.LogError("AppchargeConfig not found.");
				return;
			}

			Init(editorConfig.CheckoutPublicKey, editorConfig.Environment.ToString().ToLowerInvariant(), customerId, callback);
		}

		public void Init(string checkoutToken, string environment, string customerId, ICheckoutPurchase callback)
		{
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			Callback = callback;
			var callbackProxy = new CallbackProxy(callback);
			var configJavaObject = ConfigModelConverter.ToAndroidJavaObject(checkoutToken, environment);
			_bridgeApi.Call("init", _mainActivity, configJavaObject, customerId, "Unity " + Application.unityVersion + ", Unity SDK 2.1.0", callbackProxy);
		}

		public void OpenCheckout(string url, string sessionToken, string purchaseId)
		{
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_bridgeApi.Call("openCheckout", sessionToken, purchaseId, url);
		}

		public string GetSdkVersion()
		{
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return string.Empty;
			}

			return _bridgeApi.Call<string>("getSdkVersion");
		}

		public void GetPricePoints()
		{
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_bridgeApi.Call("getPricePoints");
		}

		public void OpenSubscriptionManager(string url)
		{
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_bridgeApi.Call("openSubscriptionManager", url);
		}

		public void ConfigurePlatform(string property, object value)
		{
			if (property.Equals("useInternalBrowser") && value is bool)
			{
				SetUseExternalBrowser(!(bool)value);
			}
		}

		public void SetUseExternalBrowser(bool useExternalBrowser)
		{
			_bridgeApi.Call("useExternalBrowser", useExternalBrowser);
		}

		private class CallbackProxy : AndroidJavaProxy
		{
			private readonly ICheckoutPurchase _callback;

			public CallbackProxy(ICheckoutPurchase callback) : base("com.appcharge.core.interfaces.ICheckoutPurchase")
			{
				_callback = callback;
			}

			public void onInitialized()
			{
				_callback.OnInitialized();
			}

			public void onInitializeFailed(AndroidJavaObject errorMessage)
			{
				_callback.OnInitializeFailed(ErrorMessageConverter.ToErrorMessage(errorMessage));
			}

			public void onPricePointsSuccess(AndroidJavaObject pricePoints)
			{
				_callback.OnPricePointsSuccess(PricePointsModelConverter.ToPricePointsModel(pricePoints));
			}

			public void onPricePointsFail(AndroidJavaObject errorMessage)
			{
				_callback.OnPricePointsFail(ErrorMessageConverter.ToErrorMessage(errorMessage));
			}

			public void onPurchaseSuccess(AndroidJavaObject orderResponse)
			{
				_callback.OnPurchaseSuccess(OrderResponseModelConverter.ToOrderResponseModel(orderResponse));
			}

			public void onPurchaseFailed(AndroidJavaObject errorMessage)
			{
				_callback.OnPurchaseFailed(ErrorMessageConverter.ToErrorMessage(errorMessage));
			}
		}
	}
}
