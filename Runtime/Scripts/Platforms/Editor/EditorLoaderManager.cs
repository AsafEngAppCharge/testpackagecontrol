#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Appcharge.PaymentLinks.Platforms.Editor {
    public class EditorLoaderManager : MonoBehaviour
    {
        private static EditorLoaderManager _instance;
        private static EventSystem _cachedEventSystem;
        private GameObject _loaderInstance;
        private Button _cancelButton;
        private Action _onCancelCallback;

        public static EditorLoaderManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("EditorLoaderManager");
                    _instance = go.AddComponent<EditorLoaderManager>();
                }
                return _instance;
            }
        }

        public void ShowLoader(Action onCancel = null)
        {
            if (_loaderInstance != null)
            {
                HideLoader();
            }

            VerifyEventSystem();

            var loaderPrefab = Resources.Load<GameObject>("AppchargeLoaderCanvas");
            if (loaderPrefab == null)
            {
                Debug.LogError("AppchargeLoaderCanvas prefab not found in Resources/Appcharge/Editor/");
                return;
            }

            _loaderInstance = Instantiate(loaderPrefab);
            _onCancelCallback = onCancel;

            _cancelButton = _loaderInstance.GetComponentInChildren<Button>();
            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(OnCancelClicked);
            }

            Debug.Log("Loading screen shown");
        }

        private void VerifyEventSystem()
        {
            if (_cachedEventSystem == null)
                _cachedEventSystem = FindObjectOfType<EventSystem>();
            
            if (_cachedEventSystem == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                _cachedEventSystem = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Debug.Log("EventSystem created for EditorLoaderManager");
            }
        }

        public void HideLoader()
        {
            if (_loaderInstance != null)
            {
                if (_cancelButton != null)
                {
                    _cancelButton.onClick.RemoveListener(OnCancelClicked);
                }

                DestroyImmediate(_loaderInstance);
                _loaderInstance = null;
                _onCancelCallback = null;
            }
        }

        private void OnCancelClicked()
        {
            Debug.Log("Order validation canceled by user");
            _onCancelCallback?.Invoke();
            HideLoader();
        }

        private void OnDestroy()
        {
            HideLoader();
        }

        public static void Cleanup()
        {
            if (_instance != null)
            {
                _instance.HideLoader();
                DestroyImmediate(_instance.gameObject);
                _instance = null;
            }
        }
    }
}
#endif
