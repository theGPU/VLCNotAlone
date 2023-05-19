using System.Diagnostics.CodeAnalysis;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.MasterServer.Services.ServerRegistry
{
    public interface IServerRegistry
    {
        void AddRegistryChangedHandler(Action handler);
        void RemoveRegistryChangedHandler(Action handler);

        bool TryGetRegisteredServer(Guid hostId, [NotNullWhen(returnValue: true)] out HostInfo? hostInfo);
        void AddRegisteredServer(Guid hostId, HostInfo hostInfo);

        bool HasOfficialServer(Guid hostId);

        IEnumerable<HostInfo> GetRegisteredServers();
        IEnumerable<HostInfo> GetPublicServers();
        IEnumerable<HostInfo> GetOfficialServers();

        IEnumerable<Guid> GetOfficalServerIds();

        void CleanupServerRegistry(DateTime now);

        void Initialize();
        void Shutdown();
        void DumpServerRegistry();
    }
}
