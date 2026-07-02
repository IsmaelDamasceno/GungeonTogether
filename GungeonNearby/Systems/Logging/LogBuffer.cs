using System.Collections.Generic;

namespace GungeonNearby.Systems.Logging
{
    /// <summary>
    /// Fixed-capacity ring buffer of recent log entries, backing the in-game log overlay.
    /// </summary>
    public static class LogBuffer
    {
        private const int Capacity = 1000;
        private static readonly LinkedList<LogEntry> _entries = new LinkedList<LogEntry>();
        private static readonly object _lock = new object();

        public static void Add(LogEntry entry)
        {
            lock (_lock)
            {
                _entries.AddLast(entry);
                if (_entries.Count > Capacity)
                    _entries.RemoveFirst();
            }
        }

        public static List<LogEntry> Snapshot()
        {
            lock (_lock)
            {
                return new List<LogEntry>(_entries);
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
            }
        }
    }
}
