using GungeonNearby.Core;
using GungeonNearby.Helpers;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Networking.Interfaces;
using GungeonNearby.Networking.Packets;
using UnityEngine;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.Networking.Proxies
{
    public class PlayerProxy : INetworkProxy
    {
        public ulong NetworkId { get; }

        private readonly bool _isLocal;

        private float _nextPositionSend;
        private const float PositionInterval = 0.25f;
        private SpriteHelper _spriteHelper;
        private tk2dBaseSprite _tkSprite;
        private SpriteRenderer _renderer;

        private GameObject _ghost;

        public PlayerProxy(ulong networkId, bool isLocal)
        {
            NetworkId = networkId;
            _isLocal = isLocal;
        }

        public static void Initialize()
        {
            Debug.Log("[PlayerProxy] Initializing patch listener for player spawned");
            GameEvents.PlayerSpawned += OnLocalPlayerSpawned;
        }

        private static void OnLocalPlayerSpawned(PlayerController player)
        {
            if (!NetworkManager.Instance.IsHost && !NetworkManager.Instance.IsClient)
            {
                return;
            }

            ulong networkId = NetworkManager.Instance.LocalPlayerId;
            if (NetworkObjectRegistry.Instance.HasProxy(networkId))
            {
                return;
            }

            Debug.Log($"[PlayerProxy] Local Player spawned, creating local proxy");

            var proxy = new PlayerProxy(networkId, isLocal: true);
            NetworkObjectRegistry.Instance.Register(proxy);
            proxy._tkSprite = player.sprite;

            var spawnPacket = new ProxySpawnedPacket
            {
                NetworkId = networkId,
                ProxyType = ProxyType.Player,
                OwnerId = networkId,
                CharacterIdentity = player.characterIdentity,
            };
            NetworkManager.Instance.RelayPlayerState(spawnPacket, reliable: true);
        }

        public void OnSpawned(INetworkPacket spawnData)
        {
            if (_isLocal)
            {
                return;
            }

            var spawn = (ProxySpawnedPacket)spawnData;

            _ghost = new GameObject($"GN_Ghost_{NetworkId}");
            Object.DontDestroyOnLoad(_ghost);

            _renderer = _ghost.AddComponent<SpriteRenderer>();
            _spriteHelper = new SpriteHelper(spawn.GetCollection());
            _renderer.sprite = _spriteHelper.GetFrame(0);
            _renderer.sortingOrder = 100;
            _ghost.transform.localScale = Vector3.one;

            Debug.Log(
                $"[PlayerProxy] Ghost for {NetworkId} ({spawn.CharacterIdentity}) sprite={_renderer.sprite?.name ?? "null"}"
            );
        }

        public void HandlePacket(INetworkPacket packet)
        {
            var payload = (PlayerPositionPacket)packet;
            if (_ghost == null)
            {
                return;
            }

            _ghost.transform.position = new Vector3(payload.Position.x, payload.Position.y, 0f);
            _renderer.sprite = _spriteHelper.GetFrame(payload.AnimationFrame);
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
                NetworkId = NetworkId,
                Position = new Vector2(p.x, p.y),
                IsGrounded = true,
                IsDodgeRolling = false,
                AnimationFrame = _tkSprite.spriteId,
            };

            NetworkManager.Instance.RelayPlayerState(packet, reliable: false);
        }

        public void OnDespawned()
        {
            if (_ghost != null)
            {
                Object.Destroy(_ghost);
            }
        }
    }
}
