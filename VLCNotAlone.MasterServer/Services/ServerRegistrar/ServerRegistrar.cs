using System.Net;
using VLCNotAlone.MasterServer.Services.ServerRegistry;
using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.MasterServer.Services.ServerRegistrar
{
    public class ServerRegistrar : IServerRegistrar
    {
        readonly ILogger _logger;
        readonly IServerRegistry _serverRegistry;
        readonly ServerBanManager _serverBanManager;

        public ServerRegistrar(ILogger logger, IServerRegistry serverRegistry, ServerBanManager serverBanManager)
        {
            _logger = logger;
            _serverRegistry = serverRegistry;
            _serverBanManager = serverBanManager;
        }

        public MasterServerError ProcessServerRegister(IPAddress? ipAddress, RegisterHostInfo registerHostInfo)
        {
            if (ipAddress == null)
                return MasterServerError.CannnotGetIPAddress;

            ipAddress = ipAddress.MapToIPv6();

            if (!_serverRegistry.HasOfficialServer(registerHostInfo.ID) && _serverBanManager.IsServerBanned(ipAddress, registerHostInfo))
                return MasterServerError.ServerBanned;

            if (_serverRegistry.TryGetRegisteredServer(registerHostInfo.ID, out var hostInfo))
            {
                if (hostInfo.Passport != hostInfo.Passport)
                {
                    return MasterServerError.InvalidPassport;
                } else
                {
                    hostInfo.IpAddress = ipAddress;
                    hostInfo.Port = registerHostInfo.Port;

                    hostInfo.Name = registerHostInfo.Name;
                    hostInfo.IsPublic = registerHostInfo.IsPublic;
                    hostInfo.ClientsCount = registerHostInfo.ClientsCount;
                    hostInfo.RoomsCount = registerHostInfo.RoomsCount;

                    hostInfo.IsOfficial = _serverRegistry.HasOfficialServer(registerHostInfo.ID);

                    hostInfo.LastUpdated = DateTime.Now;
                    return MasterServerError.NoError;
                }
            } else
            {
                var newHostInfo = registerHostInfo.ToHostInfo();
                newHostInfo.IsOfficial = _serverRegistry.HasOfficialServer(registerHostInfo.ID);
                newHostInfo.IpAddress = ipAddress;
                CheckExternallAccess(newHostInfo);
                newHostInfo.LastUpdated = DateTime.Now;
                _serverRegistry.AddRegisteredServer(registerHostInfo.ID, newHostInfo);
                return MasterServerError.NoError;
            }
        }

        private bool CheckExternallAccess(HostInfo hostInfo)
        {
            hostInfo.CheckSucceed = true;
            return true;
        }
    }
}
