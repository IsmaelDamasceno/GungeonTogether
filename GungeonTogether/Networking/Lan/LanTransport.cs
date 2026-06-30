using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GungeonTogether.Networking.Interfaces;
using GungeonTogether.Systems.Logging;
using Debug = GungeonTogether.Systems.Logging.Debug;

namespace GungeonTogether.Networking.Lan
{
    public class LanTransport : ITransport
    {
        public const int DefaultPort = 7777;

        private readonly int _localPort;
        private UdpClient _udp;
        private TcpListener _tcpListener;

        private readonly Dictionary<ulong, TcpClient> _tcpClients = new Dictionary<ulong, TcpClient>();
        private readonly Dictionary<ulong, NetworkStream> _tcpStreams = new Dictionary<ulong, NetworkStream>();
        private readonly object _tcpLock = new object();

        private readonly Queue<IncomingPacket> _incomingPackets = new Queue<IncomingPacket>();
        private readonly Queue<ulong> _sessionRequests = new Queue<ulong>();
        private readonly object _queueLock = new object();

        private bool _running;

        public ulong LocalId { get; }

        public event Action<ulong, byte[]> OnPacketReceived;
        public event Action<ulong> OnSessionRequested;

        private struct IncomingPacket
        {
            public ulong SenderId;
            public byte[] Data;
        }

        public LanTransport(int localPort)
        {
            _localPort = localPort;
            LocalId = EncodeEndpoint(new IPEndPoint(GetLocalIP(), localPort));
        }

        public static ulong EncodeEndpoint(string ip, int port) =>
            EncodeEndpoint(new IPEndPoint(IPAddress.Parse(ip), port));

        public static IPEndPoint DecodeEndpoint(ulong id)
        {
            ushort port = (ushort)(id & 0xFFFF);
            uint ip = (uint)(id >> 16);
            return new IPEndPoint(new IPAddress(BitConverter.GetBytes(ip)), port);
        }

        private static ulong EncodeEndpoint(IPEndPoint ep)
        {
            byte[] bytes = ep.Address.GetAddressBytes();
            uint ip = BitConverter.ToUInt32(bytes, 0);
            return ((ulong)ip << 16) | (ushort)ep.Port;
        }

        private static IPAddress GetLocalIP()
        {
            try
            {
                using Socket s = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Connect("8.8.8.8", 53);
                return ((IPEndPoint)s.LocalEndPoint).Address;
            }
            catch { return IPAddress.Loopback; }
        }

        public void Initialise()
        {
            _running = true;

            _udp = new UdpClient(_localPort);
            new Thread(UdpReceiveLoop) { IsBackground = true }.Start();

            _tcpListener = new TcpListener(IPAddress.Any, _localPort);
            _tcpListener.Start();
            new Thread(TcpAcceptLoop) { IsBackground = true }.Start();

            Debug.Log($"LanTransport: Listening on port {_localPort} (LocalId={LocalId}).");
        }

        public void Update()
        {
            lock (_queueLock)
            {
                while (_sessionRequests.Count > 0)
                    OnSessionRequested?.Invoke(_sessionRequests.Dequeue());

                while (_incomingPackets.Count > 0)
                {
                    IncomingPacket p = _incomingPackets.Dequeue();
                    OnPacketReceived?.Invoke(p.SenderId, p.Data);
                }
            }
        }

        public void Send(ulong targetId, byte[] data, bool reliable)
        {
            if (reliable) SendTcp(targetId, data);
            else SendUdp(targetId, data);
        }

        public void Accept(ulong remoteId) { }

        public void Close(ulong remoteId)
        {
            lock (_tcpLock)
            {
                if (_tcpClients.TryGetValue(remoteId, out TcpClient client))
                {
                    try { client.Close(); } catch { }
                    _tcpClients.Remove(remoteId);
                    _tcpStreams.Remove(remoteId);
                }
            }
        }

        public void Shutdown()
        {
            _running = false;
            try { _udp?.Close(); } catch { }
            try { _tcpListener?.Stop(); } catch { }
            lock (_tcpLock)
            {
                foreach (var client in _tcpClients.Values)
                    try { client.Close(); } catch { }
                _tcpClients.Clear();
                _tcpStreams.Clear();
            }
        }

        private void SendUdp(ulong targetId, byte[] data)
        {
            try
            {
                IPEndPoint ep = DecodeEndpoint(targetId);
                _udp.Send(data, data.Length, ep);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LanTransport: UDP send failed: {e.Message}");
            }
        }

        private void SendTcp(ulong targetId, byte[] data)
        {
            NetworkStream stream;
            lock (_tcpLock)
            {
                if (!_tcpStreams.ContainsKey(targetId))
                    ConnectTcp(targetId);
                _tcpStreams.TryGetValue(targetId, out stream);
            }

            if (stream == null) return;

            try
            {
                byte[] header = BitConverter.GetBytes(data.Length);
                lock (stream)
                {
                    stream.Write(header, 0, 4);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LanTransport: TCP send failed: {e.Message}");
                Close(targetId);
            }
        }

        private void ConnectTcp(ulong targetId)
        {
            try
            {
                IPEndPoint ep = DecodeEndpoint(targetId);
                Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult ar = socket.BeginConnect(ep, null, null);
                bool connected = ar.AsyncWaitHandle.WaitOne(2000);
                if (!connected || !socket.Connected) { socket.Close(); return; }
                socket.EndConnect(ar);

                TcpClient client = new()

                {
                    Client = socket
                };
                RegisterTcpClient(targetId, client);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LanTransport: TCP connect to {DecodeEndpoint(targetId)} failed: {e.Message}");
            }
        }

        private void RegisterTcpClient(ulong remoteId, TcpClient client)
        {
            _tcpClients[remoteId] = client;
            _tcpStreams[remoteId] = client.GetStream();
            new Thread(() => TcpReceiveLoop(remoteId, client)) { IsBackground = true }.Start();
        }

        private void UdpReceiveLoop()
        {
            while (_running)
            {
                try
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _udp.Receive(ref remote);
                    Enqueue(EncodeEndpoint(remote), data);
                }
                catch { if (!_running) return; }
            }
        }

        private void TcpAcceptLoop()
        {
            while (_running)
            {
                try
                {
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    IPEndPoint remote = (IPEndPoint)client.Client.RemoteEndPoint;
                    ulong remoteId = EncodeEndpoint(remote);

                    lock (_tcpLock)
                        RegisterTcpClient(remoteId, client);

                    lock (_queueLock)
                        _sessionRequests.Enqueue(remoteId);
                }
                catch { if (!_running) return; }
            }
        }

        private void TcpReceiveLoop(ulong remoteId, TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] header = new byte[4];

            while (_running && client.Connected)
            {
                try
                {
                    ReadExact(stream, header, 4);
                    int length = BitConverter.ToInt32(header, 0);
                    byte[] data = new byte[length];
                    ReadExact(stream, data, length);
                    Enqueue(remoteId, data);
                }
                catch { break; }
            }
        }

        private static void ReadExact(NetworkStream stream, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
                total += stream.Read(buffer, total, count - total);
        }

        private void Enqueue(ulong senderId, byte[] data)
        {
            lock (_queueLock)
                {
                    _incomingPackets.Enqueue(new IncomingPacket { SenderId = senderId, Data = data });
                }
        }
    }
}
