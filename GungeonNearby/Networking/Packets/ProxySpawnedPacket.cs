using System.IO;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Networking.Interfaces;

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
    }
}
