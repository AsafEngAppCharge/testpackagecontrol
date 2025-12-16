using System;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android {
	public static class ErrorMessageConverter
	{
		public static ErrorMessage ToErrorMessage(AndroidJavaObject javaErrorMessage)
		{
			if (javaErrorMessage == null) return null;

			return new ErrorMessage
			{
				code = GetSafeInt(javaErrorMessage, "code"),
				message = GetSafeString(javaErrorMessage, "message"),
				data = GetSafeString(javaErrorMessage, "data")
			};
		}

		private static int GetSafeInt(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return default;
			try { return obj.Get<int>(fieldName); }
			catch (Exception) { return default; }
		}

		private static string GetSafeString(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<string>(fieldName); }
			catch (Exception) { return null; }
		}
	}
}