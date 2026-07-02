using GungeonNearby.Core;
using HarmonyLib;

namespace GungeonNearby.Patches
{
    [HarmonyPatch(typeof(Foyer), nameof(Foyer.PlayerCharacterChanged))]
    static class LocalPlayerSpawnPatch
    {
        static void Postfix(PlayerController newCharacter) =>
            GameEvents.RaisePlayerSpawned(newCharacter);
    }
}
