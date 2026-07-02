using System.Collections.Generic;
using GungeonNearby.Networking.Interfaces;

namespace GungeonNearby.Networking.Proxies
{
    public class NetworkObjectRegistry
    {
        private static NetworkObjectRegistry _instance;
        public static NetworkObjectRegistry Instance => _instance ??= new NetworkObjectRegistry();

        private readonly Dictionary<ulong, INetworkProxy> _proxies = [];

        public void Register(INetworkProxy proxy) => _proxies[proxy.NetworkId] = proxy;

        public void Unregister(ulong id) => _proxies.Remove(id);

        public bool HasProxy(ulong id) => _proxies.ContainsKey(id);

        public void Dispatch(ulong id, INetworkPacket packet)
        {
            if (_proxies.TryGetValue(id, out INetworkProxy proxy))
            {
                proxy.HandlePacket(packet);
            }
        }

        public void Update()
        {
            foreach (var proxy in _proxies.Values)
            {
                proxy.Update();
            }
        }

        public void Clear()
        {
            foreach (var proxy in _proxies.Values)
            {
                proxy.OnDespawned();
            }
            _proxies.Clear();
        }
    }
}
