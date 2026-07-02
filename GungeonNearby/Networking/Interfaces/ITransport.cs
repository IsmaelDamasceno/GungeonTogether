using System;

namespace GungeonNearby.Networking.Interfaces
{
    public interface ITransport
    {
        ulong LocalId { get; }
        void Initialise();
        void Update();
        void Shutdown();
        void Send(ulong targetId, byte[] data, bool reliable);
        void Accept(ulong remoteId);
        void Close(ulong remoteId);
        event Action<ulong, byte[]> OnPacketReceived;
        event Action<ulong> OnSessionRequested;
    }
}
