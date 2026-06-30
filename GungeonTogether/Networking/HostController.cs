using System.Collections.Generic;
using GungeonTogether.Networking.Interfaces;
using GungeonTogether.Networking.Packets;
using GungeonTogether.Networking.Serialization;
using GungeonTogether.Systems.Logging;
using Debug = GungeonTogether.Systems.Logging.Debug;

namespace GungeonTogether.Networking
{
    public class HostController : IHost
    {
        private readonly ITransport _transport;
        private readonly List<ulong> _connectedClients = new List<ulong>();

        public HostController(ITransport transport)
        {
            _transport = transport;
        }

        public void Initialise()
        {
            Debug.Log("HostController Initialised.");
        }

        public void StartSession()
        {
            Debug.Log("Session Started.");
        }

        public void Update()
        {
            // Host specific logic (e.g. enemy spawning management)
        }

        public void Shutdown()
        {
            foreach (var client in _connectedClients)
                _transport.Close(client);
            _connectedClients.Clear();
        }

        public void HandleJoinRequest(ulong playerId)
        {
            if (_connectedClients.Contains(playerId)) return;

            _connectedClients.Add(playerId);
            _transport.Accept(playerId);
            Debug.Log($"[Host] Player {playerId} joined the session.");
        }

        public void HandlePlayerPosition(ulong senderId, PlayerPositionPacket packet)
        {
            if (!_connectedClients.Contains(senderId))
                HandleJoinRequest(senderId);

            Debug.Log($"[Host] Position from {senderId}: ({packet.Position.x:0.00}, {packet.Position.y:0.00})");
        }

        public void SendPacket(ulong targetId, INetworkPacket packet, bool reliable = true)
        {
            _transport.Send(targetId, PacketSerializer.Serialize(packet), reliable);
        }

        public void Broadcast(INetworkPacket packet, ulong excludeId = 0, bool reliable = true)
        {
            byte[] data = PacketSerializer.Serialize(packet);
            foreach (var client in _connectedClients)
            {
                if (client != excludeId)
                    _transport.Send(client, data, reliable);
            }
        }
    }
}
