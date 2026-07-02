using System;

namespace GungeonNearby.Systems.Logging
{
    public struct LogEntry
    {
        public readonly DateTime Time;
        public readonly LogLevel Level;
        public readonly string Category;
        public readonly string Message;

        public LogEntry(DateTime time, LogLevel level, string category, string message)
        {
            Time = time;
            Level = level;
            Category = category;
            Message = message;
        }
    }
}
