using System;
using System.Collections.Generic;
using UnityEngine;
using GungeonTogether.Core;
using GungeonTogether.Systems.Logging;
using GungeonTogether.Networking.Interfaces;
using GungeonTogether.Networking.Enums;
using GungeonTogether.Networking.Serialization;
using GungeonTogether.Networking.Packets;
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

        public const int ProtocolVersion = 1;

        public void Initialise()
        {
            try
            {
                Debug.Log("NetworkManager: Creating transport...");
                _transport = TransportFactory.Create();
                _transport.Initialise();
                _transport.OnPacketReceived += HandlePacket;
                Debug.Log($"NetworkManager: Transport ready ({_transport.GetType().Name}, LocalId={_transport.LocalId}).");

                Debug.Log("NetworkManager Initialised.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"NetworkManager: Exception during initialization: {ex.GetType().Name}: {ex.Message}");
                Debug.LogError($"NetworkManager: Stack trace: {ex.StackTrace}");
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
                        Host.HandleJoinRequest(senderId);
                        Host.SendPacket(senderId, new ConnectionAcceptedPacket
                        {
                            HostId = _transport.LocalId,
                            ProtocolVersion = ProtocolVersion,
                        }, reliable: true);
                    }
                    break;

                case PacketType.ConnectionAccepted:
                    if (IsClient)
                        Client.HandleConnectionAccepted(senderId, (ConnectionAcceptedPacket)packet);
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
