using System.IO;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Networking.Interfaces;

namespace GungeonNearby.Networking.Packets
{
    public class ConnectionAcceptedPacket : INetworkPacket
    {
        public PacketType Type => PacketType.ConnectionAccepted;
        public ulong NetworkId => 0;

        public ulong HostId;
        public ulong AssignedId;
        public int ProtocolVersion;

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(HostId);
            writer.Write(AssignedId);
            writer.Write(ProtocolVersion);
        }

        public void Deserialize(BinaryReader reader)
        {
            HostId = reader.ReadUInt64();
            AssignedId = reader.ReadUInt64();
            ProtocolVersion = reader.ReadInt32();
        }
    }
}
