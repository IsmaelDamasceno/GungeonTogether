using GungeonNearby.Core;
using GungeonNearby.Systems.Logging;
using HarmonyLib;

namespace GungeonNearby.Patches
{
    [HarmonyPatch(typeof(Foyer), nameof(Foyer.PlayerCharacterChanged))]
    static class LocalPlayerSpawnPatch
    {
        static void Postfix(PlayerController newCharacter)
        {
            Debug.Log("[LocalPlayerSpawnPatch] Local Player Spawned!");
            GameEvents.RaisePlayerSpawned(newCharacter);
        }
    }
}
