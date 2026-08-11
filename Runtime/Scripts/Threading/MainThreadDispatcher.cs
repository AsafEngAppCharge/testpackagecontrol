using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Appcharge.PaymentLinks.Threading {
    public class MainThreadDispatcher : MonoBehaviour {
        private static MainThreadDispatcher _instance;
        private static int _mainThreadId;
        private static readonly Queue<Action> _queue = new Queue<Action>();
        private static readonly List<Action> _pendingActions = new List<Action>();
        private static readonly object _lock = new object();

        public static bool Enabled { get; set; } = true;

        public static void EnsureGameObjectExists() {
            if (_instance != null || !IsMainThread()) {
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

            EnsureGameObjectExists();
            lock (_lock) {
                _queue.Enqueue(action);
            }
        }

        public static void RunSync(Action action) {
            RunSync<object>(() => {
                action();
                return null;
            });
        }

        public static T RunSync<T>(Func<T> func) {
            if (func == null) {
                return default;
            }

            if (!Enabled || IsMainThread()) {
                return func();
            }

            EnsureGameObjectExists();

            T result = default;
            Exception caught = null;
            var waitHandle = new ManualResetEventSlim(false);
            lock (_lock) {
                _queue.Enqueue(() => {
                    try {
                        result = func();
                    } catch (Exception ex) {
                        caught = ex;
                    } finally {
                        waitHandle.Set();
                    }
                });
            }

            waitHandle.Wait();
            waitHandle.Dispose();

            if (caught != null) {
                throw caught;
            }

            return result;
        }

        private void Update() {
            lock (_lock) {
                while (_queue.Count > 0) {
                    _pendingActions.Add(_queue.Dequeue());
                }
            }

            for (int i = 0; i < _pendingActions.Count; i++) {
                try {
                    _pendingActions[i]?.Invoke();
                } catch (Exception ex) {
                    Debug.LogException(ex);
                }
            }

            _pendingActions.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeMainThread() {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            EnsureGameObjectExists();
        }

        private static bool IsMainThread() {
            return Thread.CurrentThread.ManagedThreadId == _mainThreadId;
        }
    }
}
