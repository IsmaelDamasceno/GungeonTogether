using System;
using System.Collections.Generic;
using UnityEngine;
using GungeonTogether.Systems.Logging;
using GungeonTogether.Networking.Interfaces;
using GungeonTogether.Networking.Enums;
using GungeonTogether.Networking.Serialization;
using GungeonTogether.Networking.Packets;
using GungeonTogether.Networking.Steam;
using GungeonTogether.Networking.Lan;
using Debug = GungeonTogether.Systems.Logging.Debug;

namespace GungeonTogether.Networking
{
    public class NetworkManager
    {
        private static NetworkManager _instance;
        public static NetworkManager Instance => _instance ?? (_instance = new NetworkManager());

        public bool IsHost { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsConnected => CurrentRole != null;

        public INetworkRole CurrentRole { get; private set; }
        public HostController Host { get; private set; }
        public ClientController Client { get; private set; }

        private ITransport _transport;

        public ulong LocalPlayerId { get; private set; }

        public const int ProtocolVersion = 1;

        public bool IsTransportReady => _transport != null;

        public void Initialise() { }

        public void InitialiseSteam() => InitialiseTransport(new SteamTransport());

        public void InitialiseLan(int port) => InitialiseTransport(new LanTransport(port));

        private void InitialiseTransport(ITransport transport)
        {
            if (_transport != null)
            {
                Debug.LogWarning("NetworkManager: Transport already initialised, ignoring.");
                return;
            }
            try
            {
                _transport = transport;
                _transport.Initialise();
                _transport.OnPacketReceived += HandlePacket;
                LocalPlayerId = _transport.LocalId;
                Debug.Log($"NetworkManager: Transport ready ({_transport.GetType().Name}, LocalId={_transport.LocalId}).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"NetworkManager: Transport init failed: {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }

        public void Update()
        {
            _transport?.Update();
            CurrentRole?.Update();
        }

        public void StartHosting()
        {
            if (CurrentRole != null) Shutdown();

            IsHost = true;
            IsClient = false;
            LocalPlayerId = 1;

            Host = new HostController(_transport);
            Host.Initialise();
            Host.StartSession();

            CurrentRole = Host;
            Debug.Log("Started Hosting.");
        }

        public void ConnectTo(ulong hostId)
        {
            if (CurrentRole != null) Shutdown();

            IsHost = false;
            IsClient = true;

            Client = new ClientController(_transport);
            Client.Initialise();
            Client.Connect(hostId);

            CurrentRole = Client;
            Debug.Log($"Connecting to host {hostId}...");
        }

        public void Shutdown()
        {
            CurrentRole?.Shutdown();
            CurrentRole = null;
            Host = null;
            Client = null;
            IsHost = false;
            IsClient = false;
        }

        private void HandlePacket(ulong senderId, byte[] data)
        {
            try
            {
                INetworkPacket packet = PacketSerializer.Deserialize(data);
                if (packet == null) return;

                ProcessPacket(senderId, packet);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error handling packet from {senderId}: {e.Message}");
            }
        }

        private void ProcessPacket(ulong senderId, INetworkPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.ConnectionRequest:
                    if (IsHost)
                    {
                        ulong assignedId = Host.HandleJoinRequest(senderId);
                        Host.SendPacket(senderId, new ConnectionAcceptedPacket
                        {
                            HostId = LocalPlayerId,
                            AssignedId = assignedId,
                            ProtocolVersion = ProtocolVersion,
                        }, reliable: true);
                    }
                    break;

                case PacketType.ConnectionAccepted:
                    if (IsClient)
                    {
                        var accepted = (ConnectionAcceptedPacket)packet;
                        LocalPlayerId = accepted.AssignedId;
                        Client.HandleConnectionAccepted(senderId, accepted);
                    }
                    break;

                case PacketType.PlayerPosition:
                    if (IsHost)
                        Host.HandlePlayerPosition(senderId, (PlayerPositionPacket)packet);
                    break;
            }
        }

        public void SendPacket(ulong targetId, INetworkPacket packet, bool reliable = true)
        {
            CurrentRole?.SendPacket(targetId, packet, reliable);
        }
    }
}
