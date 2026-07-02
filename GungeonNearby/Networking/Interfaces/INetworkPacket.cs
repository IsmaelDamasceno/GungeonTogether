using System.IO;
using GungeonNearby.Networking.Enums;

namespace GungeonNearby.Networking.Interfaces
{
    public interface INetworkPacket
    {
        PacketType Type { get; }
        ulong NetworkId { get; }
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}
