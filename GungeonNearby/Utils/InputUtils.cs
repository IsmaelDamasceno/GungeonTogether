using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GungeonNearby.Utils
{
    public static class InputUtils
    {
        public static bool IsTyping() =>
            EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>() != null;

        public static bool IsKeyCtrl() =>
            Input.GetKey(KeyCode.LeftControl)
            || Input.GetKey(KeyCode.RightControl)
            || Input.GetKey(KeyCode.LeftCommand)
            || Input.GetKey(KeyCode.RightCommand);
    }
}
