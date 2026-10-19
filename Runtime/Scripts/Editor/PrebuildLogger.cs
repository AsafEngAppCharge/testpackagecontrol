using System;
using System.IO;
using UnityEngine;

namespace Appcharge.AppDirect.Editor
{
    public sealed class PrebuildLogger
    {
        private const string LogFileName = "AppDirectIntegrationLogs.log";
        private const string LogDirectoryName = "AppDirect";
        private readonly string _logFilePath;

        public PrebuildLogger()
        {
            _logFilePath = GetLogFilePath();
        }

        public void Log(string message, bool isError = false)
        {
            using (var writer = new StreamWriter(_logFilePath, append: true))
                writer.WriteLine(message);

            if (isError)
                throw new Exception("AppDirect automatic integration failed: " + message);
        }

        public void ClearLogs()
        {
            if (File.Exists(_logFilePath))
                File.Delete(_logFilePath);
        }

        public void PrintLog()
        {
            if (File.Exists(_logFilePath))
                Debug.Log(File.ReadAllText(_logFilePath));
        }

        private static string GetLogFilePath()
        {
            string logsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", LogDirectoryName));
            Directory.CreateDirectory(logsDir);
            return Path.Combine(logsDir, LogFileName);
        }
    }
}
