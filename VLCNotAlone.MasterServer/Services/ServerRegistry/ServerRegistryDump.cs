using System.Collections.Concurrent;
using VLCNotAlone.MasterServer.Models.Listing;

namespace VLCNotAlone.MasterServer.Services.ServerRegistry
{
    public sealed class ServerRegistryDump
    {
        public ConcurrentDictionary<Guid, HostInfo> RegisteredHosts { get; set; } = new();
    }
}
