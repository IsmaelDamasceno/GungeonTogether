using GungeonTogether.Networking.Enums;
using GungeonTogether.Networking.Interfaces;
using GungeonTogether.Networking.Packets;
using GungeonTogether.Systems.Logging;
using UnityEngine;
using Debug = GungeonTogether.Systems.Logging.Debug;

namespace GungeonTogether.Networking.Proxies
{
    public class PlayerProxy : INetworkProxy
    {
        public ulong NetworkId { get; }

        private readonly bool _isLocal;

        // Local player state
        private float _nextPositionSend;
        private const float PositionInterval = 0.25f;

        // Remote ghost state (stub until ETG spawning is understood)
        private GameObject _ghost;

        public PlayerProxy(ulong networkId, bool isLocal)
        {
            NetworkId = networkId;
            _isLocal = isLocal;
        }

        public void OnSpawned(INetworkPacket spawnData)
        {
            if (_isLocal)
            {
                Debug.Log($"[PlayerProxy] Local player {NetworkId} ready.");
            }
            else
            {
                // TODO: instantiate ghost GameObject from ETG player prefab
                Debug.Log(
                    $"[PlayerProxy] Remote player {NetworkId} joined (ghost not yet implemented)."
                );
            }
        }

        public void HandlePacket(INetworkPacket packet)
        {
            if (_isLocal)
            {
                return;
            }

            switch (packet.Type)
            {
                case PacketType.PlayerPosition:
                    var pos = (PlayerPositionPacket)packet;
                    if (_ghost != null)
                        _ghost.transform.position = new Vector3(pos.Position.x, pos.Position.y, 0f);
                    break;
            }
        }

        public void Update()
        {
            if (!_isLocal)
            {
                return;
            }

            var player = GameManager.Instance?.PrimaryPlayer;
            if (player == null)
            {
                return;
            }

            if (Time.realtimeSinceStartup < _nextPositionSend)
            {
                return;
            }
            _nextPositionSend = Time.realtimeSinceStartup + PositionInterval;

            Vector3 p = player.transform.position;
            var packet = new PlayerPositionPacket
            {
                PlayerId = NetworkId,
                Position = new Vector2(p.x, p.y),
                Velocity = Vector2.zero,
                Rotation = player.transform.eulerAngles.z,
                IsGrounded = true,
                IsDodgeRolling = false,
                AnimationState = 0,
            };

            NetworkManager.Instance.RelayPlayerState(packet, reliable: false);
        }

        public void OnDespawned()
        {
            if (_ghost != null)
                Object.Destroy(_ghost);
        }
    }
}
