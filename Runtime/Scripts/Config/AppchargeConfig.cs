using Appcharge.PaymentLinks.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Config {
    #if UNITY_IOS
    [HelpURL("https://docs.appcharge.com/sdks/payment-links/unity/integrate-with-ios")]
    #elif UNITY_ANDROID
    [HelpURL("https://docs.appcharge.com/sdks/payment-links/unity/integrate-with-android")]
    #elif UNITY_WEBGL
    [HelpURL("https://docs.appcharge.com/sdks/payment-links/unity/integrate-with-webgl")]
    #endif
    [CreateAssetMenu(fileName = "AppchargeConfig", menuName = "Appcharge/Configuration/AppchargeConfig", order = 1)]
    public class AppchargeConfig : ScriptableObject
    {   
        [Tooltip("The checkout environment. Select Sandbox for testing or Production for live operations.")]
        public AppchargeEnvironment Environment;
        
        [Tooltip("The checkout public key, located in the Publisher Dashboard. In the sidebar menu, click Settings, then select the Integration tab and copy the Checkout Public Key value.")]
        public string CheckoutPublicKey;

        //iOS Integration Settings
        [Tooltip("The browser mode to use for the checkout flow.")]
        public iOSBrowserMode iOSBrowserMode = iOSBrowserMode.SFSVC;

        [Tooltip("The domain used to redirect players back to your game, without HTTP protocol. For example, my-best-game.com .")]
        public string AssociatedDomain = "";

        [Tooltip("Whether to force the portrait orientation.")]
        public bool PortraitOrientationLock = false;

        [Tooltip("When enabled, automatically configures iOS entitlements integration (associated domains). Use the exclusion options below to disable specific operations.")]
        public bool EnableIOSEntitlementsIntegration = true;

        [Tooltip("When enabled, automatically configures iOS URL scheme integration in Info.plist. Use the exclusion options below to disable specific operations.")]
        public bool EnableIOSURLSchemeIntegration = true;

        [Tooltip("When enabled, automatically configures iOS framework integration in your Xcode project (build properties, search paths, and XCFramework embedding). Use the exclusion options below to disable specific operations.")]
        public bool EnableIOSFrameworkIntegration = true;

        [Tooltip("When enabled, skips creating the entitlements file if it doesn't exist.")]
        public bool ExcludeCreateEntitlementsFile = false;
        
        [Tooltip("When enabled, skips creating the associated-domains key in the entitlements file if it doesn't exist.")]
        public bool ExcludeCreateAssociatedDomainsKey = false;
        
        [Tooltip("When enabled, skips adding the associated domain value to the entitlements file.")]
        public bool ExcludeAddAssociatedDomain = false;

        [Tooltip("When enabled, skips adding CFBundleTypeRole to the URL scheme in Info.plist.")]
        public bool ExcludeSetURLSchemeTypeRole = false;
        
        [Tooltip("When enabled, skips adding CFBundleURLName to the URL scheme in Info.plist.")]
        public bool ExcludeSetURLSchemeName = false;
        
        [Tooltip("When enabled, skips adding CFBundleURLSchemes with the acnative URL scheme to Info.plist.")]
        public bool ExcludeAddURLScheme = false;

        [Tooltip("Exclude setting ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES to NO for UnityFramework target.")]
        public bool ExcludeSetSwiftStandardLibrariesForFramework = false;
        
        [Tooltip("Exclude adding framework search paths to the Xcode project.")]
        public bool ExcludeAddFrameworkSearchPaths = false;
        
        [Tooltip("Exclude setting LD_RUNPATH_SEARCH_PATHS build property.")]
        public bool ExcludeSetLDRunpathSearchPaths = false;
        
        [Tooltip("Exclude setting SWIFT_VERSION build property.")]
        public bool ExcludeSetSwiftVersion = false;
        
        [Tooltip("Exclude setting ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES to YES for main target.")]
        public bool ExcludeSetSwiftStandardLibrariesForMain = false;
        
        [Tooltip("Exclude setting CODE_SIGN_ENTITLEMENTS build property.")]
        public bool ExcludeSetCodeSignEntitlements = false;
        
        [Tooltip("Exclude setting CODE_SIGN_STYLE build property to Automatic.")]
        public bool ExcludeSetCodeSignStyle = false;
        
        [Tooltip("When enabled, skips adding the ACPaymentLinks.xcframework file to the Xcode project's Frameworks. Other framework configurations (build properties, search paths) will still be applied.")]
        public bool ExcludeAddXCFramework = false;

        //Android Integration Settings
        [Tooltip("The browser mode to use for the checkout flow.")]
        public AndroidBrowserMode AndroidBrowserMode = AndroidBrowserMode.TWA;

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
