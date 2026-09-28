using System;
using System.IO;
using UnityEngine;

namespace Appcharge.PaymentLinks.Editor {
    public class PrebuildLogger {
            private const string LogFileName = "AppchargeIntegrationLogs.log";
            private const string LogDirectoryName = "Appcharge";
            private readonly string logFilePath;
            private const string exceptionMessage = "Something went wrong in the automatic integration build process: ";
            private const string debugModeMessage = "*Automatic Integration Debug Mode*\n";

            public PrebuildLogger() {
                logFilePath = GetLogFilePath();
            }

            public void Log(string message, bool isError = false) {
                using StreamWriter sw = new(logFilePath, append: true);
                sw.WriteLine(message);

                if (isError)
                    throw new Exception(exceptionMessage + message);
            }

            public void ClearLogs() {
                if (File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                }
            }

            public void PrintLog() {
                Debug.Log($"Appcharge integration log path: {logFilePath}");
                if (File.Exists(logFilePath))
                {
                    Debug.Log(debugModeMessage + File.ReadAllText(logFilePath));
                }
            }

            private static string GetLogFilePath() {
                string logsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", LogDirectoryName));
                Directory.CreateDirectory(logsDir);
                return Path.Combine(logsDir, LogFileName);
            }
    }
}
