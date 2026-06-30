namespace GungeonTogether.Networking.Enums
{
    public enum PacketType : byte
    {
        None = 0,
        InstancePayload = 1,
        ConnectionRequest = 2,
        ConnectionAccepted = 3,
        // Add more packet types here
    }
}
