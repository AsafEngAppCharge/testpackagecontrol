#if UNITY_EDITOR
using System;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

namespace Appcharge.PaymentLinks.Platforms.Editor
{
    /// <summary>
    /// EditorWindow that provides a better checkout experience in the Unity Editor.
    /// Opens the checkout URL in a dedicated window with better UX than external browser.
    /// </summary>
    public class CheckoutWebViewWindow : EditorWindow
    {
        private string _checkoutUrl;
        private static CheckoutWebViewWindow _instance;
        private Process _browserProcess;

        public static CheckoutWebViewWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetWindow<CheckoutWebViewWindow>("Appcharge Checkout");
                    _instance.minSize = new Vector2(800, 600);
                }
                return _instance;
            }
        }

        public static void OpenCheckout(string url)
        {
            var window = Instance;
            window._checkoutUrl = url;
            window.Show();
            window.Focus();
            
            // Open URL in default browser (better than Application.OpenURL for tracking)
            window.OpenURLInBrowser();
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_checkoutUrl))
            {
                EditorGUILayout.HelpBox("No checkout URL provided.", MessageType.Warning);
                return;
            }

            // Header
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Appcharge Checkout", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // URL display
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Checkout URL:", GUILayout.Width(100));
            EditorGUILayout.TextField(_checkoutUrl, EditorStyles.textField);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Instructions
            EditorGUILayout.HelpBox(
                "The checkout page has been opened in your default browser. " +
                "Complete the payment process there. This window will remain open to track the order status.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Open in Browser", GUILayout.Height(30)))
            {
                OpenURLInBrowser();
            }
            
            if (GUILayout.Button("Copy URL", GUILayout.Height(30)))
            {
                EditorGUIUtility.systemCopyBuffer = _checkoutUrl;
                EditorUtility.DisplayDialog("URL Copied", "Checkout URL has been copied to clipboard.", "OK");
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            // Status section
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Waiting for order validation...\n" +
                "The order status will be updated automatically once the payment is completed.",
                MessageType.None);
        }

        private void OpenURLInBrowser()
        {
            if (string.IsNullOrEmpty(_checkoutUrl))
                return;

            try
            {
                #if UNITY_EDITOR_OSX
                // On macOS, use 'open' command for better integration
                Process.Start("open", _checkoutUrl);
                #elif UNITY_EDITOR_WIN
                // On Windows, use start command
                Process.Start(new ProcessStartInfo
                {
                    FileName = _checkoutUrl,
                    UseShellExecute = true
                });
                #elif UNITY_EDITOR_LINUX
                // On Linux, try xdg-open
                Process.Start("xdg-open", _checkoutUrl);
                #else
                // Fallback
                Application.OpenURL(_checkoutUrl);
                #endif
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to open checkout URL: {ex.Message}");
                // Fallback to Unity's method
                Application.OpenURL(_checkoutUrl);
            }
        }

        private void OnDestroy()
        {
            if (_browserProcess != null && !_browserProcess.HasExited)
            {
                try
                {
                    _browserProcess.Close();
                }
                catch { }
            }
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
#endif

