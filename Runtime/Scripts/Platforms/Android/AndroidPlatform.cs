using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android
{
	public class AndroidPlatform : ICheckoutPlatform
	{
		private AndroidJavaObject _bridgeApi;
		private AndroidJavaObject _mainActivity;
		private const string UNITY_SDK_VERSION = "2.2.0";
		private AndroidBrowserMode _browserMode = AndroidBrowserMode.TWA;
		private bool _debugMode = false;
		private bool _portraitOrientationLock = false;
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

			_browserMode = editorConfig.AndroidBrowserMode;
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

			AppchargeConfig editorConfig = ConfigUtility.GetConfig();
			if (editorConfig != null)
			{
				_browserMode = editorConfig.AndroidBrowserMode;
			}

			Callback = callback;
			var callbackProxy = new CallbackProxy(callback, this);
			var configJavaObject = ConfigModelConverter.ToAndroidJavaObject(checkoutToken, environment);
			_bridgeApi.Call("init", _mainActivity, configJavaObject, customerId, "Unity " + Application.unityVersion + ", Unity SDK " + UNITY_SDK_VERSION, callbackProxy);
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
			if (property.Equals("browserMode") && value is AndroidBrowserMode)
			{
				SetBrowserMode((AndroidBrowserMode)value);
			}

			if (property.Equals("debugMode") && value is bool)
			{
				SetDebugMode((bool)value);
			}

			if (property.Equals("portraitOrientationLock") && value is bool)
			{
				SetPortraitOrientationLock((bool)value);
			}
		}

		private void SetBrowserMode(AndroidBrowserMode mode)
		{
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_browserMode = mode;
			string modeString = mode.ToString().ToLowerInvariant();
			_bridgeApi.Call<string>("setBrowserMode", modeString);
		}

		private void SetDebugMode(bool debugMode) {
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_debugMode = debugMode;
			_bridgeApi.Call<bool>("setDebugMode", debugMode);
		}

		private void SetPortraitOrientationLock(bool portraitOrientationLock) {
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_portraitOrientationLock = portraitOrientationLock;
			_bridgeApi.Call<bool>("setPortraitOrientationLock", portraitOrientationLock);
		}

		public void OnInitialized()
		{
			SetBrowserMode(_browserMode);
			SetDebugMode(_debugMode);
			SetPortraitOrientationLock(_portraitOrientationLock);
		}

		private class CallbackProxy : AndroidJavaProxy
		{
			private readonly ICheckoutPurchase _callback;
			private readonly AndroidPlatform _platform;

			public CallbackProxy(ICheckoutPurchase callback, AndroidPlatform platform) : base("com.appcharge.core.interfaces.ICheckoutPurchase")
			{
				_callback = callback;
				_platform = platform;
			}

			public void onInitialized()
			{
				_callback.OnInitialized();
				_platform.OnInitialized();
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
