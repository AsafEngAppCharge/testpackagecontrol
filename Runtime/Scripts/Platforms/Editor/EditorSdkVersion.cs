#if UNITY_EDITOR
using Appcharge.PaymentLinks.Config;
using Appcharge.PaymentLinks.Interfaces;
using Appcharge.PaymentLinks.Platforms.Base;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorSdkVersion : BaseSdkVersion
    {
        public EditorSdkVersion(ICheckoutPlatform platform, EditorPlatform editorPlatform) : base(platform)
        {
        }

        public override string GetSdkVersion()
        {
            return SdkVersion.UnitySdkVersion;
        }
    }
}
#endif
