using System.IO;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Networking.Interfaces;
using UnityEngine;

namespace GungeonNearby.Networking.Packets
{
    public class PlayerPositionPacket : INetworkPacket
    {
        public PacketType Type => PacketType.InstancePayload;
        public ulong NetworkId { get; set; }

        public Vector2 Position;
        public bool IsGrounded;
        public bool IsDodgeRolling;
        public int AnimationFrame;

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(NetworkId);
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(IsGrounded);
            writer.Write(IsDodgeRolling);
            writer.Write(AnimationFrame);
        }

        public void Deserialize(BinaryReader reader)
        {
            NetworkId = reader.ReadUInt64();
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            IsGrounded = reader.ReadBoolean();
            IsDodgeRolling = reader.ReadBoolean();
            AnimationFrame = reader.ReadInt32();
        }
    }
}
