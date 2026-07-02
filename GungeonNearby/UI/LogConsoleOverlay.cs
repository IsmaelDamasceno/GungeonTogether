using System.Collections.Generic;
using GungeonNearby.Systems.Logging;
using UnityEngine;

namespace GungeonNearby.UI
{
    /// <summary>
    /// In-game overlay showing recent log entries, toggled with F10. Avoids relying on the
    /// BepInEx cmd console for reading logs while playing.
    /// </summary>
    public class LogConsoleOverlay : MonoBehaviour
    {
        private bool _visible;
        private Vector2 _scroll;
        private string _filter = "";
        private bool _showDebug = true;
        private bool _showInfo = true;
        private bool _showWarning = true;
        private bool _showError = true;
        private Rect _windowRect = new(40, 40, 720, 420);
        private GUIStyle _lineStyle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                _visible = !_visible;
            }
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }

            _windowRect = GUILayout.Window(
                GetInstanceID(),
                _windowRect,
                DrawWindow,
                "GungeonNearby Log Console (F10 to toggle)"
            );
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(40));
            _filter = GUILayout.TextField(_filter, GUILayout.Width(200));
            _showDebug = GUILayout.Toggle(_showDebug, "Debug");
            _showInfo = GUILayout.Toggle(_showInfo, "Info");
            _showWarning = GUILayout.Toggle(_showWarning, "Warning");
            _showError = GUILayout.Toggle(_showError, "Error");
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
                LogBuffer.Clear();
            GUILayout.EndHorizontal();

            if (_lineStyle == null)
            {
                _lineStyle = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true };
            }

            _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(340));
            List<LogEntry> entries = LogBuffer.Snapshot();
            for (int i = 0; i < entries.Count; i++)
            {
                LogEntry entry = entries[i];
                if (!PassesFilter(entry))
                    continue;

                string color = LevelColor(entry.Level);
                string line =
                    $"<color={color}>[{entry.Time:HH:mm:ss}] [{entry.Level}] [{entry.Category}]</color> {entry.Message}";
                GUILayout.Label(line, _lineStyle);
            }
            GUILayout.EndScrollView();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private bool PassesFilter(LogEntry entry)
        {
            switch (entry.Level)
            {
                case LogLevel.Debug when !_showDebug:
                case LogLevel.Info when !_showInfo:
                case LogLevel.Warning when !_showWarning:
                case LogLevel.Error when !_showError:
                    return false;
            }

            if (string.IsNullOrEmpty(_filter))
                return true;

            return entry.Message.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) >= 0
                || entry.Category.IndexOf(_filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string LevelColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return "grey";
                case LogLevel.Warning:
                    return "yellow";
                case LogLevel.Error:
                    return "red";
                default:
                    return "white";
            }
        }
    }
}
