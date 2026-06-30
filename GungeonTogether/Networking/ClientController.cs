using GungeonTogether.Networking.Interfaces;
using GungeonTogether.Networking.Packets;
using GungeonTogether.Networking.Serialization;
using GungeonTogether.Systems.Logging;
using UnityEngine;
using Debug = GungeonTogether.Systems.Logging.Debug;

namespace GungeonTogether.Networking
{
    public class ClientController : IClient
    {
        private readonly ITransport _transport;
        private ulong _hostId;

        public bool IsConnected { get; private set; }

        private float _nextPositionSendTime;
        private const float PositionSendInterval = 0.25f;

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

        public void Update()
        {
            if (!IsConnected)
            {
                return;
            }
            if (Time.realtimeSinceStartup < _nextPositionSendTime)
            {
                return;
            }
            _nextPositionSendTime = Time.realtimeSinceStartup + PositionSendInterval;

            var player = GameManager.Instance ? GameManager.Instance.PrimaryPlayer : null;
            if (player == null)
            {
                return;
            }

            Vector3 pos3 = player.transform.position;
            var packet = new PlayerPositionPacket
            {
                PlayerId = _transport.LocalId,
                Position = new Vector2(pos3.x, pos3.y),
                Velocity = Vector2.zero,
                Rotation = player.transform.eulerAngles.z,
                IsGrounded = true,
                IsDodgeRolling = false,
                AnimationState = 0,
            };

            SendPacket(_hostId, packet, reliable: false);
            Debug.Log(
                $"[Client] Sent position packet: ({packet.Position.x:0.00}, {packet.Position.y:0.00})"
            );
        }

        public void HandleConnectionAccepted(ulong senderId, ConnectionAcceptedPacket packet)
        {
            if (senderId != _hostId)
                return;

            if (packet.ProtocolVersion != NetworkManager.ProtocolVersion)
                Debug.LogWarning(
                    $"[Client] Protocol mismatch. Host={packet.ProtocolVersion} Local={NetworkManager.ProtocolVersion}"
                );

            IsConnected = true;
            _nextPositionSendTime = 0;
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
