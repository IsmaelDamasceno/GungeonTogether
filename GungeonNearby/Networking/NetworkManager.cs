using System;
using System.Collections.Generic;
using GungeonNearby.Networking.Enums;
using GungeonNearby.Networking.Interfaces;
using GungeonNearby.Networking.Lan;
using GungeonNearby.Networking.Packets;
using GungeonNearby.Networking.Proxies;
using GungeonNearby.Networking.Serialization;
using GungeonNearby.Networking.Steam;
using GungeonNearby.Systems.Logging;
using UnityEngine;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.Networking
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
                Debug.Log(
                    $"NetworkManager: Transport ready ({_transport.GetType().Name}, LocalId={_transport.LocalId})."
                );
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"NetworkManager: Transport init failed: {ex.GetType().Name}: {ex.Message}"
                );
                throw;
            }
        }

        public void Update()
        {
            _transport?.Update();
            CurrentRole?.Update();
            NetworkObjectRegistry.Instance.Update();
        }

        public void RelayPlayerState(INetworkPacket packet, bool reliable = false)
        {
            if (IsHost)
            {
                Host?.Broadcast(packet, reliable: reliable);
            }
            else
            {
                Client?.SendPacket(0, packet, reliable);
            }
        }

        public void StartHosting()
        {
            if (CurrentRole != null)
                Shutdown();

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
            if (CurrentRole != null)
                Shutdown();

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
            NetworkObjectRegistry.Instance.Clear();
        }

        private void HandlePacket(ulong senderId, byte[] data)
        {
            try
            {
                INetworkPacket packet = PacketSerializer.Deserialize(data);
                if (packet == null)
                    return;

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
                        Host.SendPacket(
                            senderId,
                            new ConnectionAcceptedPacket
                            {
                                HostId = LocalPlayerId,
                                AssignedId = assignedId,
                                ProtocolVersion = ProtocolVersion,
                            },
                            reliable: true
                        );
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

                case PacketType.ProxySpawned:
                    var spawnPacket = (ProxySpawnedPacket)packet;
                    if (!NetworkObjectRegistry.Instance.HasProxy(spawnPacket.NetworkId))
                    {
                        INetworkProxy proxy = CreateProxy(spawnPacket);
                        if (proxy != null)
                        {
                            NetworkObjectRegistry.Instance.Register(proxy);
                            proxy.OnSpawned(spawnPacket);
                        }
                    }
                    if (IsHost)
                        Host.Broadcast(spawnPacket, excludeId: senderId, reliable: true);
                    break;

                case PacketType.InstancePayload:
                    if (!NetworkObjectRegistry.Instance.HasProxy(packet.NetworkId))
                    {
                        Debug.LogError(
                            $"[NetworkManager] No proxy registered for NetworkId={packet.NetworkId}, dropping packet."
                        );
                        break;
                    }
                    NetworkObjectRegistry.Instance.Dispatch(packet.NetworkId, packet);
                    if (IsHost)
                        Host.Broadcast(packet, excludeId: senderId, reliable: false);
                    break;
            }
        }

        private INetworkProxy CreateProxy(ProxySpawnedPacket packet)
        {
            switch (packet.ProxyType)
            {
                case ProxyType.Player:
                    return new PlayerProxy(packet.NetworkId, isLocal: false);
                default:
                    Debug.LogWarning(
                        $"[NetworkManager] No proxy class for ProxyType={packet.ProxyType}"
                    );
                    return null;
            }
        }

        public void SendPacket(ulong targetId, INetworkPacket packet, bool reliable = true)
        {
            CurrentRole?.SendPacket(targetId, packet, reliable);
        }
    }
}
