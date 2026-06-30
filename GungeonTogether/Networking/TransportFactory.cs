using System;
using GungeonTogether.Networking.Interfaces;
using GungeonTogether.Networking.Lan;
using GungeonTogether.Networking.Steam;

namespace GungeonTogether.Networking
{
    public static class TransportFactory
    {
        public static ITransport Create()
        {
            string[] args = Environment.GetCommandLineArgs();
            int lanPort = ParseLanPort(args);

            if (lanPort > 0)
            {
                return new LanTransport(lanPort);
            }
            return new SteamTransport();
        }

        private static int ParseLanPort(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--gt-lan-port" && int.TryParse(args[i + 1], out int port))
                    return port;
            }
            return 0;
        }
    }
}
