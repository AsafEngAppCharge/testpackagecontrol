using System;
using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android {
	public static class ErrorMessageConverter
	{
		public static ErrorMessage ToErrorMessage(AndroidJavaObject javaErrorMessage)
		{
			if (javaErrorMessage == null) return null;

			try
			{
				int code = default;
				string message = null;

				if (ConvertUtility.TryCallInstanceInt(javaErrorMessage, "getCode", "()I", out int c))
					code = c;
				if (ConvertUtility.TryCallInstanceString(javaErrorMessage, "getMessage", "()Ljava/lang/String;", out string m) && !string.IsNullOrEmpty(m))
					message = m;

				return new ErrorMessage { code = code, message = message };
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"ErrorMessageConverter: {ex.Message}");
				return new ErrorMessage { code = 9999, message = null };
			}
		}
	}
}
