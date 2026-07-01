using System;

namespace GungeonTogether.Core
{
    public static class GameEvents
    {
        public static event Action<PlayerController> PlayerSpawned;

        public static void RaisePlayerSpawned(PlayerController player) =>
            PlayerSpawned?.Invoke(player);
    }
}
