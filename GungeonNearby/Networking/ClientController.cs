using GungeonNearby.Networking.Interfaces;
using GungeonNearby.Networking.Packets;
using GungeonNearby.Networking.Serialization;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.Networking
{
    public class ClientController : IClient
    {
        private readonly ITransport _transport;
        private ulong _hostId;

        public bool IsConnected { get; private set; }

        public ClientController(ITransport transport)
        {
            _transport = transport;
        }

        public void Initialise()
        {
            Debug.Log("ClientController Initialised.");
        }

        public void Connect(ulong hostId)
        {
            _hostId = hostId;
            IsConnected = false;

            Debug.Log($"Sending connection request to {hostId}...");

            SendPacket(
                _hostId,
                new ConnectionRequestPacket
                {
                    ClientId = _transport.LocalId,
                    ProtocolVersion = NetworkManager.ProtocolVersion,
                },
                reliable: true
            );
        }

        public void Update() { }

        public void HandleConnectionAccepted(ulong senderId, ConnectionAcceptedPacket packet)
        {
            if (senderId != _hostId)
            {
                return;
            }

            if (packet.ProtocolVersion != NetworkManager.ProtocolVersion)
            {
                Debug.LogWarning(
                    $"[Client] Protocol mismatch. Host={packet.ProtocolVersion} Local={NetworkManager.ProtocolVersion}"
                );
            }

            IsConnected = true;
            Debug.Log($"[Client] Connection accepted by host {senderId}.");
        }

        public void Shutdown() => Disconnect();

        public void Disconnect()
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            Debug.Log("Disconnected from host.");
        }

        public void SendPacket(ulong targetId, INetworkPacket packet, bool reliable = true)
        {
            if (targetId == 0)
                targetId = _hostId;
            _transport.Send(targetId, PacketSerializer.Serialize(packet), reliable);
        }
    }
}
