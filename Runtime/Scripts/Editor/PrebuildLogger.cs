using System;
using System.IO;
using UnityEngine;

namespace Appcharge.PaymentLinks.Editor {
    public class PrebuildLogger {
            private readonly string logFilePath = Path.Combine(Application.dataPath, "BuildLogs.txt");
            private const string exceptionMessage = "Something went wrong in the automatic integration build process: ";
            private const string debugModeMessage = "*Automatic Integration Debug Mode*\n";

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
                Debug.Log("PrebuildLogger: PrintLog");
                if (File.Exists(logFilePath))
                {
                    Debug.Log(debugModeMessage + File.ReadAllText(logFilePath));
                }
            }
    }
}