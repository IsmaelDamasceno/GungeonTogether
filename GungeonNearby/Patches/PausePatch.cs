using HarmonyLib;

namespace GungeonNearby.Patches
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.OnApplicationFocus))]
    class PausePatch
    {
        static bool Prefix(bool focusStatus)
        {
            return focusStatus;
        }
    }
}
