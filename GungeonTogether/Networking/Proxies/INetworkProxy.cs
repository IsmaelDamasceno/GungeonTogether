using GungeonTogether.Networking.Interfaces;

namespace GungeonTogether.Networking.Proxies
{
    public interface INetworkProxy
    {
        ulong NetworkId { get; }
        void OnSpawned(INetworkPacket spawnData);
        void HandlePacket(INetworkPacket packet);
        void OnDespawned();
        void Update();
    }
}
