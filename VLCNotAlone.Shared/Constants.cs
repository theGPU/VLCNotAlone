using Newtonsoft.Json;
using System;
using VLCNotAlone.Shared.Utils.Serializer;

namespace VLCNotAlone.Shared
{
    public class Constants
    {
        public const ushort MasterServerPort = 7132;
        public static readonly string MasterServerUrl = $"https://localhost:{MasterServerPort}";

        public const int MasterServerRefreshTimeSeconds = 30;
        public const int MasterServerTimeoutSeconds = MasterServerRefreshTimeSeconds + 15;
        public const byte MasterServerProtocolVersion = 1;

        //public static readonly TimeSpan ServerToClientKeepAliveInterval = TimeSpan.FromSeconds(15);
        public static readonly TimeSpan ClientToServerKeepAliveInterval = TimeSpan.FromSeconds(5);
        public static readonly JsonSerializerSettings MasterServerJsonSerializerSettings = new JsonSerializerSettings();

        static Constants()
        {
            MasterServerJsonSerializerSettings.Converters.Add(new IPEndPointConverter());
            MasterServerJsonSerializerSettings.Converters.Add(new IPAddressConverter());
        }
    }
}
