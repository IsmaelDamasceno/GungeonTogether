using GungeonNearby.Core;
using HarmonyLib;
using UnityEngine;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.Patches
{
    [HarmonyPatch(typeof(Foyer), nameof(Foyer.PlayerCharacterChanged))]
    static class LocalPlayerSpawnPatch
    {
        static void Postfix(PlayerController newCharacter)
        {
            Debug.Log(
                $"[LocalPlayerSpawnPatch] Local Player Spawned! name: {newCharacter.gameObject.name}, root name: {newCharacter.transform.root.gameObject.name}"
            );
            var components = newCharacter.gameObject.GetComponents<MonoBehaviour>();
            foreach (var c in components)
            {
                Debug.Log($"comp: {c.GetType().Name}");
            }
            GameEvents.RaisePlayerSpawned(newCharacter);
        }
    }
}
