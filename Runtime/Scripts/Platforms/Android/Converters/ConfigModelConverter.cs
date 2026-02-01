using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android {
	public static class ConfigModelConverter
	{
		public static AndroidJavaObject ToAndroidJavaObject(string checkoutPublicKey, string environment)
		{
			using (var configModelClass = new AndroidJavaClass("com.appcharge.paymentlinks.models.ConfigModel"))
			{
				return new AndroidJavaObject(
					"com.appcharge.paymentlinks.models.ConfigModel",
					checkoutPublicKey,
					environment
				);
			}
		}
	}
}