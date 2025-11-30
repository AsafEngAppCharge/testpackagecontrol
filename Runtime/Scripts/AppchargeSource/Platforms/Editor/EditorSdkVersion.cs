#if UNITY_EDITOR
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Platforms.Base;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorSdkVersion : BaseSdkVersion
    {
        private EditorPlatform _editorPlatform;
        
        public EditorSdkVersion(ICheckoutPlatform platform, EditorPlatform editorPlatform) : base(platform)
        {
            _editorPlatform = editorPlatform;
        }

        public override string GetSdkVersion()
        {
            return "0.0.0-editor";
        }
    }
}
#endif
