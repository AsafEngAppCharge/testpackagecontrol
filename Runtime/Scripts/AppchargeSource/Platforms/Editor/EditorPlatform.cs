#if UNITY_EDITOR
using Appcharge.PaymentLinks.Platforms.Base;
using Appcharge.PaymentLinks.Platforms.Editor.Models;
using UnityEngine;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorPlatform : Platform
    {
        public EditorBootResponse BootData;
        public string CustomerId;
        public string CheckoutPublicKey;
        public string Environment;

        private static CoroutineRunner _sharedCoroutineRunner;

        /// <summary>
        /// Gets or creates a shared CoroutineRunner instance for all editor platform operations.
        /// This prevents creating multiple GameObjects for coroutine execution.
        /// </summary>
        public static CoroutineRunner SharedCoroutineRunner
        {
            get
            {
                if (_sharedCoroutineRunner == null)
                {
                    var runnerObject = new GameObject("EditorPlatformCoroutineRunner");
                    _sharedCoroutineRunner = runnerObject.AddComponent<CoroutineRunner>();
                    Object.DontDestroyOnLoad(runnerObject);
                }
                return _sharedCoroutineRunner;
            }
        }

        protected override void InitializeComponents()
        {
            _init = new EditorInit(this, this);
            _openCheckout = new EditorOpenCheckout(this, this);
            _sdkVersion = new EditorSdkVersion(this, this);
            _pricePoints = new EditorPricePoints(this, this);
            _subscription = new EditorSubscription(this, this);
        }

        public override void ConfigurePlatform(string property, object value) { }
    }

    /// <summary>
    /// Simple MonoBehaviour component used to run coroutines in the Editor platform.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour { }
}
#endif