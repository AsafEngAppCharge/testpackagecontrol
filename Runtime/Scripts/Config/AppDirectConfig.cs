using Appcharge.AppDirect.Models;
using UnityEngine;

namespace Appcharge.AppDirect.Config
{
    [CreateAssetMenu(fileName = "AppDirectConfig", menuName = "Appcharge/Configuration/AppDirectConfig", order = 1)]
    public class AppDirectConfig : ScriptableObject
    {
        [Tooltip("Checkout public key from the Publisher Dashboard (Settings > Integration).")]
        public string CheckoutPublicKey;

        [Tooltip("Sandbox, Production, or Staging.")]
        public AppDirectEnvironment Environment = AppDirectEnvironment.Sandbox;

        [Tooltip("Marshal SDK calls and callbacks onto the Unity main thread.")]
        public bool EnableMainThreadDispatcher = true;

        [Tooltip("Print integration and dispatcher logs.")]
        public bool EnableDebugMode = false;
 
        [Tooltip("Inject the AppDirect Maven dependency, engine meta-data, and Gradle AndroidX flags during an Android build.")]
        public bool EnableIntegrationOptions = true;

        [Tooltip("Skip writing android.useAndroidX=true into gradleTemplate.properties.")]
        public bool ExcludeAndroidX = false;

        [Tooltip("Skip writing android.enableJetifier=true into gradleTemplate.properties.")]
        public bool ExcludeJetifier = false;

        [Tooltip("Skip the androidx.core:core-ktx dependency (FileProvider).")]
        public bool ExcludeCoreKtx = false;

        [Tooltip("Skip the androidx.appcompat dependency.")]
        public bool ExcludeAppCompat = false;

        [Tooltip("Skip kotlinx-serialization-json.")]
        public bool ExcludeKotlinSerializationJson = false;

        [Tooltip("Skip kotlinx-coroutines-core and kotlinx-coroutines-android.")]
        public bool ExcludeKotlinCoroutines = false;

        [Tooltip("Skip androidx.work:work-runtime-ktx.")]
        public bool ExcludeWorkRuntime = false;

        [Tooltip("Skip androidx.lifecycle:lifecycle-process.")]
        public bool ExcludeLifecycleProcess = false;

        [Tooltip("Skip com.google.android.material.")]
        public bool ExcludeMaterial = false;

        [Tooltip("When the native AAR manifest merge is not enough, also write install permissions, FileProvider, and InstallPermissionActivity into the host AndroidManifest.xml.")]
        public bool EnsureNativeManifestEntries = false;
    }
}
