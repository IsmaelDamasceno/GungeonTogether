namespace GungeonNearby.Systems.Logging
{
    public static class Debug
    {
        public static void Log(object message) => Logger.LogInfo(message);

        public static void Log(string category, object message) => Logger.LogInfo(category, message);

        public static void LogWarning(object message) => Logger.LogWarning(message);

        public static void LogWarning(string category, object message) =>
            Logger.LogWarning(category, message);

        public static void LogError(object message) => Logger.LogError(message);

        public static void LogError(string category, object message) =>
            Logger.LogError(category, message);
    }
}
