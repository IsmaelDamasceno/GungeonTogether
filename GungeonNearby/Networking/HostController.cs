using System.Collections.Generic;
using GungeonNearby.Networking.Interfaces;
using GungeonNearby.Networking.Packets;
using GungeonNearby.Networking.Serialization;
using GungeonNearby.Systems.Logging;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.Networking
{
    public class HostController : IHost
    {
        private readonly ITransport _transport;
        private readonly List<ulong> _connectedClients = new List<ulong>();
        private readonly Dictionary<ulong, ulong> _assignedIds = new Dictionary<ulong, ulong>();
        private ulong _nextClientId = 2; // host is always 1

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

        public ulong HandleJoinRequest(ulong transportId)
        {
            if (_assignedIds.TryGetValue(transportId, out ulong existing))
                return existing;

            ulong assignedId = _nextClientId++;
            _assignedIds[transportId] = assignedId;
            _connectedClients.Add(transportId);
            _transport.Accept(transportId);
            Debug.Log($"[Host] Player {transportId} joined, assigned id={assignedId}.");
            return assignedId;
        }

        public void HandlePlayerPosition(ulong senderId, PlayerPositionPacket packet)
        {
            if (!_connectedClients.Contains(senderId))
                HandleJoinRequest(senderId);

            Debug.Log(
                $"[Host] Position from {senderId}: ({packet.Position.x:0.00}, {packet.Position.y:0.00})"
            );
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
                {
                    _transport.Send(client, data, reliable);
                }
            }
        }
    }
}
