using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GungeonNearby.Patches
{
    [HarmonyPatch]
    static class InputBlockPatch
    {
        private static bool IsTyping() =>
            EventSystem.current?.currentSelectedGameObject?.GetComponent<InputField>() != null;

        [HarmonyPatch(typeof(Input), nameof(Input.GetKey), typeof(KeyCode))]
        [HarmonyPrefix]
        static bool GetKey(ref bool __result)
        {
            if (!IsTyping())
                return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(Input), nameof(Input.GetKey), typeof(string))]
        [HarmonyPrefix]
        static bool GetKeyStr(ref bool __result)
        {
            if (!IsTyping())
                return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
        [HarmonyPrefix]
        static bool GetKeyDown(ref bool __result)
        {
            if (!IsTyping())
                return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(string))]
        [HarmonyPrefix]
        static bool GetKeyDownStr(ref bool __result)
        {
            if (!IsTyping())
                return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), typeof(KeyCode))]
        [HarmonyPrefix]
        static bool GetKeyUp(ref bool __result)
        {
            if (!IsTyping())
                return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUp), typeof(string))]
        [HarmonyPrefix]
        static bool GetKeyUpStr(ref bool __result)
        {
            if (!IsTyping())
                return true;
            __result = false;
            return false;
        }
    }
}
