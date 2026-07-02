using GungeonNearby.Networking;

namespace GungeonNearby.Networking.Interfaces
{
    public interface INetworkRole
    {
        void Initialise();
        void Update();
        void Shutdown();
        void SendPacket(ulong targetId, INetworkPacket packet, bool reliable = true);
    }

    public interface IHost : INetworkRole
    {
        void StartSession();
        ulong HandleJoinRequest(ulong transportId);
        void Broadcast(INetworkPacket packet, ulong excludeId = 0, bool reliable = true);
    }

    public interface IClient : INetworkRole
    {
        void Connect(ulong hostId);
        void Disconnect();
        bool IsConnected { get; }
    }
}
