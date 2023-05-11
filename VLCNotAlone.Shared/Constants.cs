using Newtonsoft.Json;
using System;
using VLCNotAlone.Shared.Utils.Serializer;

namespace VLCNotAlone.Shared
{
    public class Constants
    {
        public const int MasterServerRefreshTimeSeconds = 30;
        public const int MasterServerTimeoutSeconds = MasterServerRefreshTimeSeconds + 15;
        public const byte MasterServerProtocolVersion = 1;

        public static readonly JsonSerializerSettings MasterServerJsonSerializerSettings = new JsonSerializerSettings();

        static Constants()
        {
            MasterServerJsonSerializerSettings.Converters.Add(new IPEndPointConverter());
            MasterServerJsonSerializerSettings.Converters.Add(new IPAddressConverter());
        }
    }
}
