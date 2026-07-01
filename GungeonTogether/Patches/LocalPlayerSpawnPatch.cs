using GungeonTogether.Core;
using HarmonyLib;

namespace GungeonTogether.Patches
{
    [HarmonyPatch(typeof(Foyer), nameof(Foyer.PlayerCharacterChanged))]
    static class LocalPlayerSpawnPatch
    {
        static void Postfix(PlayerController newCharacter) =>
            GameEvents.RaisePlayerSpawned(newCharacter);
    }
}
