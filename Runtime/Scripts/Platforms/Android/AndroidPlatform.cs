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
		private AndroidErrorHandler _errorHandler;
		public ICheckoutPurchase Callback { get; set; }
		private CallbackProxy _callbackProxy;
		private BrowserMode _browserMode = BrowserMode.Internal;
		private bool _debugMode = false;
		private bool _portraitOrientationLock = false;

		private void EnsureInitialized()
		{
			if (_errorHandler == null)
			{
				_errorHandler = new AndroidErrorHandler();
				_errorHandler.Initialize();
			}

			if (_bridgeApi != null && _mainActivity != null) 
				return;

			using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				_mainActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				_bridgeApi = new AndroidJavaObject("com.appcharge.paymentlinks.BridgeAPI");
			}
		}

		public void Init(ICheckoutPurchase callback)
		{
			AppchargeConfig editorConfig = ConfigUtility.GetConfig();
			if (editorConfig == null)
			{
				Debug.LogError("AppchargeConfig not found.");
				return;
			}

			_browserMode = editorConfig.BrowserMode;
			Init(editorConfig.CheckoutPublicKey, editorConfig.Environment.ToString().ToLowerInvariant(), callback);
		}

		public void Init(string checkoutToken, string environment, ICheckoutPurchase callback)
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
				_browserMode = editorConfig.BrowserMode;
			}

			Callback = callback;
			_callbackProxy ??= new CallbackProxy(this);

			var configJavaObject = ConfigModelConverter.ToAndroidJavaObject(checkoutToken, environment);
			_bridgeApi.Call("init", _mainActivity, configJavaObject, _callbackProxy);
		}

		public void OpenCheckout(string purchaseId, string parsedUrl, string customerId)
		{
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_bridgeApi.Call("openCheckout", purchaseId, parsedUrl, customerId);
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

		public void ConfigurePlatform(string property, object value)
		{
			if (property.Equals("browserMode") && value is BrowserMode)
			{
				SetBrowserMode((BrowserMode)value);
			}

			if (property.Equals("debugMode") && value is bool)
			{
				SetDebugMode((bool)value);
			}

			if (property.Equals("portraitOrientationLock") && value is bool)
			{
				SetPortraitOrientationLock((bool)value);
			}

			if (property.Equals("setCheckoutServiceMode") && value is bool)
			{
				SetCheckoutServiceMode((bool)value);
			}
		}

		private void SetBrowserMode(BrowserMode mode)
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

		private void SetCheckoutServiceMode(bool checkoutServiceMode) {
			EnsureInitialized();
			if (_bridgeApi == null)
			{
				Debug.LogError("BridgeAPI is not initialized.");
				return;
			}

			_bridgeApi.Call<bool>("setCheckoutServiceMode", checkoutServiceMode);
		}

		public void OnInitialized()
		{
			SetBrowserMode(_browserMode);
			SetDebugMode(_debugMode);
			SetPortraitOrientationLock(_portraitOrientationLock);
		}

		private class CallbackProxy : AndroidJavaProxy
		{
			private readonly AndroidPlatform _platform;

			public CallbackProxy(AndroidPlatform platform) : base("com.appcharge.paymentlinks.interfaces.ICheckoutPurchase")
			{
				_platform = platform;
			}

			public void onInitialized()
			{
				_platform.Callback?.OnInitialized();
				_platform.OnInitialized();
			}

			public void onInitializeFailed(AndroidJavaObject errorMessage)
			{
				_platform.Callback?.OnInitializeFailed(ErrorMessageConverter.ToErrorMessage(errorMessage));
			}

			public void onPricePointsSuccess(AndroidJavaObject pricePoints)
			{
				_platform.Callback?.OnPricePointsSuccess(PricePointsModelConverter.ToPricePointsModel(pricePoints));
			}

			public void onPricePointsFail(AndroidJavaObject errorMessage)
			{
				_platform.Callback?.OnPricePointsFail(ErrorMessageConverter.ToErrorMessage(errorMessage));
			}

			public void onPurchaseSuccess(AndroidJavaObject orderResponse)
			{
				_platform.Callback?.OnPurchaseSuccess(OrderResponseModelConverter.ToOrderResponseModel(orderResponse));
			}

			public void onPurchaseFailed(AndroidJavaObject errorMessage, AndroidJavaObject orderResponse)
			{
				_platform.Callback?.OnPurchaseFailed(ErrorMessageConverter.ToErrorMessage(errorMessage), OrderResponseModelConverter.ToOrderResponseModel(orderResponse));
			}

			public void onPurchaseCanceled(AndroidJavaObject errorMessage, AndroidJavaObject orderResponse)
			{
				_platform.Callback?.OnPurchaseCanceled(ErrorMessageConverter.ToErrorMessage(errorMessage), OrderResponseModelConverter.ToOrderResponseModel(orderResponse));
			}
		}
	}
}
