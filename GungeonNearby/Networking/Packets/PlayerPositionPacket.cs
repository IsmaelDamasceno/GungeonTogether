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
        public Vector2 Velocity;
        public float Rotation;
        public bool IsGrounded;
        public bool IsDodgeRolling;
        public int AnimationState;

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(NetworkId);
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Velocity.x);
            writer.Write(Velocity.y);
            writer.Write(Rotation);
            writer.Write(IsGrounded);
            writer.Write(IsDodgeRolling);
            writer.Write(AnimationState);
        }

        public void Deserialize(BinaryReader reader)
        {
            NetworkId = reader.ReadUInt64();
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Rotation = reader.ReadSingle();
            IsGrounded = reader.ReadBoolean();
            IsDodgeRolling = reader.ReadBoolean();
            AnimationState = reader.ReadInt32();
        }
    }
}
