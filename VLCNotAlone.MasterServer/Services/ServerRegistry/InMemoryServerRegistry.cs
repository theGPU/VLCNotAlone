using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.MasterServer.Services.ServerRegistry
{
    public class InMemoryServerRegistry : IServerRegistry
    {
        readonly ILogger _logger;

        const string ServerRegistryDumpPath = "ServerRegistryDump.json";
        const string OfficialServerIdsPath = "OfficialServerIds.txt";

        ConcurrentDictionary<Guid, HostInfo> registeredHosts = new();
        public event Action? ServerRegistryChanged;

        public InMemoryServerRegistry(ILogger logger)
        {
            _logger = logger;
        }

        public void AddRegistryChangedHandler(Action handler) => this.ServerRegistryChanged += handler;
        public void RemoveRegistryChangedHandler(Action handler) => this.ServerRegistryChanged -= handler;

        public bool TryGetRegisteredServer(Guid hostId, [NotNullWhen(returnValue: true)] out HostInfo? hostInfo)
        {
            if (this.registeredHosts.TryGetValue(hostId, out hostInfo))
                return true;

            return false;
        }

        public void AddRegisteredServer(Guid hostId, HostInfo hostInfo)
        {
            var isNew = !this.registeredHosts.ContainsKey(hostId);

            this.registeredHosts[hostId] = hostInfo;

            if (isNew)
                this.ServerRegistryChanged?.Invoke();
        }

        public bool HasOfficialServer(Guid hostId) => this.GetOfficalServerIds().Contains(hostId);

        public IEnumerable<HostInfo> GetRegisteredServers() => this.registeredHosts.Values;
        public IEnumerable<HostInfo> GetPublicServers() => this.registeredHosts.Values.Where(x => x.IsPublic);
        public IEnumerable<HostInfo> GetOfficialServers()
        {
            foreach (var id in this.GetOfficalServerIds())
            {
                if (this.registeredHosts.TryGetValue(id, out var hostInfo))
                    yield return hostInfo;
            }
        }

        public IEnumerable<Guid> GetOfficalServerIds() => File.ReadAllLines(OfficialServerIdsPath).Select(Guid.Parse);

        private static bool CleanupServerList(ConcurrentDictionary<Guid, HostInfo> hosts, DateTime now)
        {
            var changed = false;

            foreach (var host in hosts)
            {
                var secondsSinceUpdate = (now - host.Value.LastUpdated).TotalSeconds;
                if (secondsSinceUpdate >= Constants.MasterServerTimeoutSeconds)
                    if (hosts.TryRemove(host.Key, out _))
                        changed = true;
            }

            return changed;
        }

        public void CleanupServerRegistry(DateTime now)
        {
            var hostsChanged = CleanupServerList(this.registeredHosts, now);

            if (hostsChanged)
                this.ServerRegistryChanged?.Invoke();
        }

        public void Initialize()
        {
            if (File.Exists(ServerRegistryDumpPath))
            {
                _logger.LogInformation("Loading server registry from disk");
                var json = File.ReadAllText(ServerRegistryDumpPath);
                try
                {
                    var dump = JsonConvert.DeserializeObject<ServerRegistryDump>(json, Constants.MasterServerJsonSerializerSettings);

                    if (dump != null)
                    {
                        this.registeredHosts = dump.RegisteredHosts;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load server registry from disk");
                }
            }
        }

        public void Shutdown() => this.DumpServerRegistry();

        public void DumpServerRegistry()
        {
            _logger.LogInformation("Writing server registry cache to disk");
            var dump = new ServerRegistryDump
            {
                RegisteredHosts = this.registeredHosts
            };

            File.WriteAllText(ServerRegistryDumpPath, JsonConvert.SerializeObject(dump, Constants.MasterServerJsonSerializerSettings));
        }
    }
}
