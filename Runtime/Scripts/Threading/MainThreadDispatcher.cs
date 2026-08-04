using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Appcharge.PaymentLinks.Threading {
    public class MainThreadDispatcher : MonoBehaviour {
        private static MainThreadDispatcher _instance;
        private static int _mainThreadId;
        private static readonly Queue<Action> _queue = new Queue<Action>();
        private static readonly object _lock = new object();

        public static bool Enabled { get; set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static void EnsureExists() {
            if (_instance != null) {
                return;
            }

            var gameObject = new GameObject("AppchargeMainThreadDispatcher");
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<MainThreadDispatcher>();
        }

        public static void Run(Action action) {
            if (action == null) {
                return;
            }

            if (!Enabled || IsMainThread()) {
                action();
                return;
            }

            EnsureExists();
            lock (_lock) {
                _queue.Enqueue(action);
            }
        }

        private static bool IsMainThread() {
            return Thread.CurrentThread.ManagedThreadId == _mainThreadId;
        }

        private void Update() {
            while (true) {
                Action action = null;
                lock (_lock) {
                    if (_queue.Count == 0) {
                        break;
                    }

                    action = _queue.Dequeue();
                }

                try {
                    action?.Invoke();
                } catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }
        }
    }
}
