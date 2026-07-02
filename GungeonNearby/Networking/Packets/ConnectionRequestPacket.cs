using System.IO;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Networking.Interfaces;

namespace GungeonNearby.Networking.Packets
{
    public class ConnectionRequestPacket : INetworkPacket
    {
        public PacketType Type => PacketType.ConnectionRequest;
        public ulong NetworkId => 0;

        public ulong ClientId;
        public int ProtocolVersion;

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ClientId);
            writer.Write(ProtocolVersion);
        }

        public void Deserialize(BinaryReader reader)
        {
            ClientId = reader.ReadUInt64();
            ProtocolVersion = reader.ReadInt32();
        }
    }
}
