using System;
using System.Text;
using UnityEngine;

namespace AREducation.Data
{
    public static class DiagnosticsLogger
    {
        private const string LogKey = "diagnostics_log_v1";
        private const int MaxChars = 12000;

        public static void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            string line = $"{DateTime.UtcNow:o} {message}";
            string current = PlayerPrefs.GetString(LogKey, "");
            string next = string.IsNullOrEmpty(current) ? line : current + "\n" + line;
            if (next.Length > MaxChars)
                next = next.Substring(next.Length - MaxChars);

            PlayerPrefs.SetString(LogKey, next);
            PlayerPrefs.Save();
            Debug.Log($"[AR Education] {message}");
        }

        public static string GetLog() => PlayerPrefs.GetString(LogKey, "");

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(LogKey);
            PlayerPrefs.Save();
        }

        public static string BuildDeviceSnapshot()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"App Version: {Application.version}");
            sb.AppendLine($"Unity: {Application.unityVersion}");
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"Device: {SystemInfo.deviceModel}");
            sb.AppendLine($"OS: {SystemInfo.operatingSystem}");
            sb.AppendLine($"Graphics: {SystemInfo.graphicsDeviceName}");
            return sb.ToString();
        }
    }
}
