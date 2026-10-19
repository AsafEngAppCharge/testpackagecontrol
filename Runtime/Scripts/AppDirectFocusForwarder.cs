using UnityEngine;

namespace Appcharge.AppDirect
{
    internal sealed class AppDirectFocusForwarder : MonoBehaviour
    {
        public static void EnsureExists()
        {
            // if (FindObjectOfType<AppDirectFocusForwarder>() != null)
            //     return;

            // var gameObject = new GameObject("AppDirectFocusForwarder");
            // gameObject.hideFlags = HideFlags.HideAndDontSave;
            // DontDestroyOnLoad(gameObject);
            // gameObject.AddComponent<AppDirectFocusForwarder>();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            //AppDirectController.Instance.OnAppFocusChanged(hasFocus);
        }
    }
}
