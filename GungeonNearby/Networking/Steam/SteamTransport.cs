using System;
using GungeonNearby.Networking.Interfaces;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.Networking.Steam
{
    public class SteamTransport : ITransport
    {
        private readonly SteamP2PManager _p2p;
        private SteamLobbyManager _lobby;

        public ulong LocalId => _p2p.LocalSteamID;

        public event Action<ulong, byte[]> OnPacketReceived;
        public event Action<ulong> OnSessionRequested;

        public SteamTransport()
        {
            _p2p = SteamP2PManager.Instance;
        }

        public void Initialise()
        {
            _p2p.Initialise();
            _p2p.OnPacketReceived += (id, data) => OnPacketReceived?.Invoke(id, data);
            _p2p.OnP2PSessionRequest += id => OnSessionRequested?.Invoke(id);

            _lobby = SteamLobbyManager.Instance;
            _lobby.Initialise();
        }


        public void Update()
        {
            _lobby?.Update();
            _p2p.Update();
        }

        public void Shutdown() { }

        public void Send(ulong targetId, byte[] data, bool reliable) =>
            _p2p.SendPacket(targetId, data, reliable);

        public void Accept(ulong remoteId)
        {
            try
            {
                if (SteamReflectionHelper.AcceptP2PSessionMethod != null)
                {
                    object steamIdObj = SteamReflectionHelper.CreateCSteamID(remoteId);
                    SteamReflectionHelper.AcceptP2PSessionMethod.Invoke(null, new object[] { steamIdObj });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SteamTransport: Failed to accept P2P session with {remoteId}: {e.Message}");
            }
        }

        public void Close(ulong remoteId)
        {
            try
            {
                if (SteamReflectionHelper.CloseP2PSessionMethod != null)
                {
                    object steamIdObj = SteamReflectionHelper.CreateCSteamID(remoteId);
                    SteamReflectionHelper.CloseP2PSessionMethod.Invoke(null, new object[] { steamIdObj });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SteamTransport: Failed to close P2P session with {remoteId}: {e.Message}");
            }
        }
    }
}
