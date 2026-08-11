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
        public static bool DebugLogging { get; set; }

        private const string LogTag = "[Appcharge MainThread]";

        public static void EnsureGameObjectExists() {
            if (_instance != null || !IsMainThread()) {
                return;
            }

            var gameObject = new GameObject("AppchargeMainThreadDispatcher");
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
            _instance = gameObject.AddComponent<MainThreadDispatcher>();
            LogDebug("EnsureGameObjectExists: dispatcher GameObject created");
        }

        public static void Run(Action action) {
            if (action == null) {
                return;
            }

            if (!Enabled) {
                LogDebug("Run: dispatcher disabled, executing inline");
                action();
                return;
            }

            if (IsMainThread()) {
                LogDebug("Run: on main thread, executing inline");
                action();
                return;
            }

            LogDebug("Run: off main thread, enqueueing");
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

            if (!Enabled) {
                LogDebug("RunSync: dispatcher disabled, executing inline");
                return func();
            }

            if (IsMainThread()) {
                LogDebug("RunSync: on main thread, executing inline");
                return func();
            }

            LogDebug("RunSync: off main thread, enqueueing and blocking");
            EnsureGameObjectExists();

            T result = default;
            Exception caught = null;
            var waitHandle = new ManualResetEventSlim(false);
            lock (_lock) {
                _queue.Enqueue(() => {
                    try {
                        LogDebug("RunSync: executing queued work on main thread");
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

            if (_pendingActions.Count > 0) {
                LogDebug($"Update: draining {_pendingActions.Count} queued action(s) on main thread");
            }

            for (int i = 0; i < _pendingActions.Count; i++) {
                try {
                    LogDebug($"Update: executing queued action {_pendingActions.Count - i} remaining");
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
            LogDebug("InitializeMainThread: main thread id captured");
            EnsureGameObjectExists();
        }

        private static bool IsMainThread() {
            return Thread.CurrentThread.ManagedThreadId == _mainThreadId;
        }

        private static void LogDebug(string message) {
            if (!DebugLogging) {
                return;
            }

            Debug.Log($"{LogTag} {message} | threadId={Thread.CurrentThread.ManagedThreadId}, mainThreadId={_mainThreadId}, onMainThread={IsMainThread()}");
        }
    }
}
