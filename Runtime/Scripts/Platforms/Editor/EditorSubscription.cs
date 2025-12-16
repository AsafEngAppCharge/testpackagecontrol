#if UNITY_EDITOR
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Platforms.Base;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorSubscription : BaseSubscription
    {
        private EditorPlatform _editorPlatform;
        
        public EditorSubscription(ICheckoutPlatform platform, EditorPlatform editorPlatform) : base(platform)
        {
            _editorPlatform = editorPlatform;
        }

        public override void OpenSubscriptionManager(string url)
        {
            Application.OpenURL(url);
        }
    }
}
#endif
