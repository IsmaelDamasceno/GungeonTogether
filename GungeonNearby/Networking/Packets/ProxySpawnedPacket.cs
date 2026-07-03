using System.IO;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Networking.Interfaces;
using GungeonNearby.Systems.Logging;

namespace GungeonNearby.Networking.Packets
{
    public class ProxySpawnedPacket : INetworkPacket
    {
        public PacketType Type => PacketType.ProxySpawned;
        public ulong NetworkId { get; set; }

        public ProxyType ProxyType;
        public ulong OwnerId;
        public PlayableCharacters CharacterIdentity;

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(NetworkId);
            writer.Write((byte)ProxyType);
            writer.Write(OwnerId);
            writer.Write((int)CharacterIdentity);
        }

        public void Deserialize(BinaryReader reader)
        {
            NetworkId = reader.ReadUInt64();
            ProxyType = (ProxyType)reader.ReadByte();
            OwnerId = reader.ReadUInt64();
            CharacterIdentity = (PlayableCharacters)reader.ReadInt32();
        }

        public static readonly System.Collections.Generic.Dictionary<
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

        public string GetCollection()
        {
            return GetCollection(CharacterIdentity);
        }

        public static string GetCollection(PlayableCharacters identity)
        {
            if (!CharacterCollections.TryGetValue(identity, out string collectionName))
            {
                Debug.LogWarning($"[PlayerProxy] No collection mapping for character '{identity}'");
                return null;
            }
            return collectionName;
        }
    }
}
