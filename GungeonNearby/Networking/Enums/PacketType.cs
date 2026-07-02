namespace GungeonNearby.Networking.Enums
{
    public enum PacketType : byte
    {
        None = 0,
        InstancePayload = 1,
        ConnectionRequest = 2,
        ConnectionAccepted = 3,
        ProxySpawned = 4,
        // Add more packet types here
    }
}
