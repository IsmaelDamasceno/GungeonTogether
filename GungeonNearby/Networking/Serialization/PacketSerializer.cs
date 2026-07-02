using System;
using System.IO;
using GungeonNearby.Networking;
using GungeonNearby.Networking.Interfaces;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Systems.Logging;

namespace GungeonNearby.Networking.Serialization
{
    public static class PacketSerializer
    {
        public static byte[] Serialize(INetworkPacket packet)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write((byte)packet.Type);
                packet.Serialize(writer);
                return ms.ToArray();
            }
        }

        public static INetworkPacket Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                PacketType type = (PacketType)reader.ReadByte();
                INetworkPacket packet = PacketFactory.Create(type);
                
                if (packet != null)
                {
                    packet.Deserialize(reader);
                    return packet;
                }
                
                Debug.LogWarning($"Unknown packet type: {type}");
                return null;
            }
        }
    }
}
