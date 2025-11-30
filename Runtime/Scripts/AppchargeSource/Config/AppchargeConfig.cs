using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Config {
    [HelpURL("https://docs.appcharge.com/sdks/appdirect/mobile-checkout-sdk/unity/introduction")]
    [CreateAssetMenu(fileName = "AppchargeConfig", menuName = "Appcharge/Configuration/AppchargeConfig", order = 1)]
    public class AppchargeConfig : ScriptableObject
    {   
        [Tooltip("The checkout environment. Select Sandbox for testing or Production for live operations.")]
        public AppchargeEnvironment Environment;
        
        [Tooltip("The checkout public key, located in the Publisher Dashboard. In the sidebar menu, click Settings, then select the Integration tab and copy the Checkout Public Key value.")]
        public string CheckoutPublicKey;

        //iOS Integration Settings
        [Tooltip("Whether the checkout opens in internal browser mode.")]
        public bool UseInternalBrowser = true;

        [Tooltip("Whether to force the portrait orientation.")]
        public bool PortraitOrientationLock = false;

        [Tooltip("Whether to automatically include and sign the Appcharge iOS frameworks in your Xcode project.")]
        public bool AddFrameworkToXcodeProject = true;

        [Tooltip("The domain used to redirect players back to your game, without HTTP protocol. For example, my-best-game.com .")]
        public string AssociatedDomain = "";

        [Tooltip("Whether to enable deeplink support when opening the checkout in an embedded browser.")]
        public bool AddURLScheme = true;

        //Android Integration Settings
        [Tooltip("Exclude the 'useAndroidX' property from the 'gradleTemplate.properties' file.")]
        public bool ExcludeAndroidX = false;
        [Tooltip("Exclude the 'enableJetifier' property from the 'gradleTemplate.properties' file.")]
        public bool ExcludeJetifier = false;

        [Tooltip("Exclude the 'AppCompat' dependency from the mainTemplate.gradle file.")]
        public bool ExcludeAppcompat = false;
        [Tooltip("Exclude the 'AndroidbrowserHelper' dependency from the mainTemplate.gradle file.")]
        public bool ExcludeAndroidbrowser = false;
        [Tooltip("Exclude the Kotlin packages dependencies from the mainTemplate.gradle file.")]
        public bool ExcludeKotlin = false;

        [Tooltip("Exclude the Internet permission from the AndroidManifest.xml file.")]
        public bool ExcludeInternetPermission = false;

        [Tooltip("Exclude the `<queries>` block from the AndroidManifest.xml file.")]
        public bool ExcludeQueriesBlock = false;

        [Tooltip("Exclude the entire Appcharge Checkout Activity configuration from the AndroidManifest.xml file.")]
        public bool ExcludeAppchargeActivity = false;

        [Tooltip("Exclude the intent filters from the Appcharge Checkout Activity.")]
        public bool ExcludeAppchargeActivityIntentFilters = false;

        [Tooltip("Exclude the `android:exported` attribute in the Checkout Activity.")]
        public bool ExcludeExportedAttribute = false;

        [Tooltip("Exclude the `<data android:scheme=\"acnative-{gameNameLowerCase}\">` entry in the intent filter.")]
        public bool ExcludeCustomScheme = false;

        [Tooltip("Exclude the `<data android:host=\"action\">` entry in the intent filter.")]
        public bool ExcludeCustomHost = false;

        [Tooltip("Exclude the `<data android:scheme=\"https\">` entry in the intent filter.")]
        public bool ExcludeHttpsSchemeInActivity = false;

        [Tooltip("Exclude the `<tools:ignore=\"DiscouragedApi\">` in the Checkout Activity.")]
        public bool ExcludeDiscouragedApiTool = false;

        [Header("General Auto Integration Settings")]
        [Tooltip("Whether the SDK automatically applies required platform-specific configurations during the build process. If disabled, the automatic modifications will be turned off and you’ll need to configure them manually.")]
        public bool EnableIntegrationOptions = true;

        [Tooltip("Enable this to print a summary of the automatic integration changes after the build."
        + " \nFor debugging purposes.")]
        public bool EnableDebugMode = false;
    }
}
