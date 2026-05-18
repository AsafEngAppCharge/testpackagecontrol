using System;
using System.Globalization;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android {
	internal static class ConvertUtility
	{
		internal static bool TryCallInstanceInt(AndroidJavaObject obj, string methodName, string jniSignature, out int value)
		{
			value = default;
			if (obj == null) return false;
			IntPtr raw = obj.GetRawObject();
			IntPtr clazz = AndroidJNI.GetObjectClass(raw);
			try
			{
				IntPtr mid = AndroidJNI.GetMethodID(clazz, methodName, jniSignature);
				if (mid == IntPtr.Zero)
					return false;
				value = AndroidJNI.CallIntMethod(raw, mid, null);
				return true;
			}
			finally
			{
				AndroidJNI.DeleteLocalRef(clazz);
			}
		}

		internal static bool TryCallInstanceString(AndroidJavaObject obj, string methodName, string jniSignature, out string value)
		{
			value = null;
			if (obj == null) return false;
			IntPtr raw = obj.GetRawObject();
			IntPtr clazz = AndroidJNI.GetObjectClass(raw);
			try
			{
				IntPtr mid = AndroidJNI.GetMethodID(clazz, methodName, jniSignature);
				if (mid == IntPtr.Zero)
					return false;
				IntPtr jstr = AndroidJNI.CallObjectMethod(raw, mid, null);
				if (jstr == IntPtr.Zero)
					return false;
				try
				{
					value = AndroidJNI.GetStringUTFChars(jstr);
					return true;
				}
				finally
				{
					AndroidJNI.DeleteLocalRef(jstr);
				}
			}
			finally
			{
				AndroidJNI.DeleteLocalRef(clazz);
			}
		}

		internal static string GetSafeString(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<string>(fieldName); }
			catch { return null; }
		}

		internal static int? GetSafeInt(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<int>(fieldName); }
			catch
			{
				try { return (int)obj.Get<long>(fieldName); } catch { }
				try { return (int)obj.Get<double>(fieldName); } catch { }
				var s = GetSafeString(obj, fieldName);
				if (int.TryParse(s, out var i)) return i;
				if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
					return (int)d;
				return null;
			}
		}

		internal static double? GetSafeDouble(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<double>(fieldName); }
			catch
			{
				var s = GetSafeString(obj, fieldName);
				if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
					return d;
				return null;
			}
		}

		internal static long? GetSafeLong(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<long>(fieldName); }
			catch
			{
				try { return (long)obj.Get<int>(fieldName); } catch { }
				try { return (long)obj.Get<double>(fieldName); } catch { }
				var s = GetSafeString(obj, fieldName);
				return ParseLong(s);
			}
		}

		internal static bool? GetSafeBool(AndroidJavaObject obj, string fieldName)
		{
			if (obj == null) return null;
			try { return obj.Get<bool>(fieldName); } catch { return null; }
		}

		internal static long? ParseLong(string s)
		{
			if (string.IsNullOrEmpty(s)) return null;
			if (long.TryParse(s, out var l)) return l;
			if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
				return (long)d;
			return null;
		}
	}
}
