using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Android
{
    public class AndroidErrorHandler
    {
        public const string PlayerPrefsKeyLastDiagnostic = "AC_PAYMENT_LINKS_UNITY_CRASH_LOG";

        private const int MaxStoredLength = 16384;

        private const string JavaNamespacePrefix   = "com.appcharge.paymentlinks.";
        private const string CSharpNamespacePrefix = "Appcharge.PaymentLinks.";

        private const int AtPrefixLength = 3;
        private static readonly int MinSdkFrameLength = AtPrefixLength + CSharpNamespacePrefix.Length;

        private bool _initialized;
        private readonly object _initLock = new object();

        public void Initialize()
        {
            lock (_initLock)
            {
                if (_initialized) 
					return;

                _initialized = true;
                Application.logMessageReceived += OnLogMessageReceived;
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error) 
				return;

            if (!IsFromSdk(condition, stackTrace)) 
			    return;

            try
            {
                string timestamp = System.DateTime.UtcNow.ToString("o");
                string payload   = $"{timestamp}\n{type}\n{condition}\n{stackTrace ?? string.Empty}";

                if (payload.Length > MaxStoredLength)
                    payload = payload.Substring(0, MaxStoredLength);

                PlayerPrefs.SetString(PlayerPrefsKeyLastDiagnostic, payload);
                PlayerPrefs.Save();
            }
            catch { }
        }

        private static bool IsFromSdk(string condition, string stackTrace)
            => ContainsSdkFrame(condition) || ContainsSdkFrame(stackTrace);

        private static bool ContainsSdkFrame(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < MinSdkFrameLength)
                return false;

            int scanLimit = text.Length - MinSdkFrameLength;

            for (int i = 0; i <= scanLimit; i++)
            {
                if (!IsAtPrefix(text, i)) continue;

                int namespaceStart = i + AtPrefixLength;
                if (HasPrefixAt(text, namespaceStart, JavaNamespacePrefix))   return true;
                if (HasPrefixAt(text, namespaceStart, CSharpNamespacePrefix)) return true;
            }

            return false;
        }

        private static bool IsAtPrefix(string text, int i)
            => string.CompareOrdinal(text, i, "at ", 0, AtPrefixLength) == 0;

        private static bool HasPrefixAt(string text, int offset, string prefix)
        {
            if (offset + prefix.Length > text.Length) return false;
            if (text[offset] != prefix[0])            return false;

            return string.CompareOrdinal(text, offset, prefix, 0, prefix.Length) == 0;
        }
    }
}