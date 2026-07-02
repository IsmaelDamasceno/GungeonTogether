using GungeonNearby.Core;
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

            var spawnPacket = new ProxySpawnedPacket
            {
                NetworkId = networkId,
                ProxyType = ProxyType.Player,
                OwnerId = networkId,
                CharacterIdentity = player.characterIdentity,
            };
            proxy.OnSpawned(spawnPacket);
            NetworkManager.Instance.RelayPlayerState(spawnPacket, reliable: true);
        }

        public void OnSpawned(INetworkPacket spawnData)
        {
            if (_isLocal)
                return;

            var spawn = (ProxySpawnedPacket)spawnData;

            _ghost = new GameObject($"GN_Ghost_{NetworkId}");
            Object.DontDestroyOnLoad(_ghost);

            var sr = _ghost.AddComponent<SpriteRenderer>();
            sr.sprite = TryBuildSprite(spawn.CharacterIdentity) ?? CreateSquareSprite();
            sr.sortingOrder = 100;
            _ghost.transform.localScale = Vector3.one;

            Debug.Log(
                $"[PlayerProxy] Ghost for {NetworkId} ({spawn.CharacterIdentity}) sprite={sr.sprite?.name ?? "null"}"
            );
        }

        private static readonly System.Collections.Generic.Dictionary<
            PlayableCharacters,
            string
        > CharacterCollections = new()
        {
            { PlayableCharacters.Soldier, "Marine" },
            { PlayableCharacters.Pilot, "SpaceRogue" },
            { PlayableCharacters.Convict, "Convict" },
            { PlayableCharacters.Robot, "Robot" },
            { PlayableCharacters.Guide, "Guide" },
            { PlayableCharacters.CoopCultist, "CoopCultist" },
            { PlayableCharacters.Gunslinger, "Gunslinger_Collection" },
            { PlayableCharacters.Bullet, "Playable_Bullet_Man" },
        };

        private static Sprite TryBuildSprite(PlayableCharacters identity)
        {
            try
            {
                if (!CharacterCollections.TryGetValue(identity, out string collectionName))
                {
                    Debug.LogWarning(
                        $"[PlayerProxy] No collection mapping for character '{identity}'"
                    );
                    return null;
                }
                var collection = ETGMod.Assets.FindCollectionOfName(collectionName);
                if (collection == null)
                {
                    Debug.LogWarning($"[PlayerProxy] No collection found for '{collectionName}'");
                    return null;
                }

                var def = collection.spriteDefinitions[0];
                var tex = def.material?.mainTexture as Texture2D;
                if (tex == null)
                {
                    Debug.LogWarning(
                        $"[PlayerProxy] Collection '{collectionName}' has no texture on def[0]"
                    );
                    return null;
                }

                // Convert UV coords to pixel rect
                var uvs = def.uvs;
                float x = uvs[0].x * tex.width;
                float y = uvs[0].y * tex.height;
                float w = Mathf.Abs(uvs[1].x - uvs[0].x) * tex.width;
                float h = Mathf.Abs(uvs[2].y - uvs[0].y) * tex.height;

                Debug.Log(
                    $"[PlayerProxy] Atlas '{tex.name}' {tex.width}x{tex.height}, rect=({x},{y},{w},{h})"
                );
                return Sprite.Create(tex, new Rect(x, y, w, h), new Vector2(0.5f, 0.5f), 16f);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayerProxy] TryBuildSprite failed: {ex.Message}");
                return null;
            }
        }

        private static Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);
        }

        public void HandlePacket(INetworkPacket packet)
        {
            var pos = (PlayerPositionPacket)packet;
            if (_ghost != null)
                _ghost.transform.position = new Vector3(pos.Position.x, pos.Position.y, 0f);
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
