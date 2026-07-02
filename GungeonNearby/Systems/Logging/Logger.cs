using System;
using BepInEx.Logging;

namespace GungeonNearby.Systems.Logging
{
    public static class Logger
    {
        private const string DefaultCategory = "General";

        private static ManualLogSource _logSource;

        public static void Initialise(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        public static void LogInfo(object data) => Log(LogLevel.Info, DefaultCategory, data);

        public static void LogInfo(string category, object data) => Log(LogLevel.Info, category, data);

        public static void LogWarning(object data) => Log(LogLevel.Warning, DefaultCategory, data);

        public static void LogWarning(string category, object data) =>
            Log(LogLevel.Warning, category, data);

        public static void LogError(object data) => Log(LogLevel.Error, DefaultCategory, data);

        public static void LogError(string category, object data) =>
            Log(LogLevel.Error, category, data);

        public static void LogDebug(object data) => Log(LogLevel.Debug, DefaultCategory, data);

        public static void LogDebug(string category, object data) =>
            Log(LogLevel.Debug, category, data);

        private static void Log(LogLevel level, string category, object data)
        {
            string message = data?.ToString() ?? "null";
            LogBuffer.Add(new LogEntry(DateTime.Now, level, category, message));

            switch (level)
            {
                case LogLevel.Debug:
                    _logSource?.LogDebug(message);
                    break;
                case LogLevel.Info:
                    _logSource?.LogInfo(message);
                    break;
                case LogLevel.Warning:
                    _logSource?.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _logSource?.LogError(message);
                    break;
            }
        }
    }
}
