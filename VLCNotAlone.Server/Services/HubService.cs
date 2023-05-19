using Microsoft.AspNetCore.SignalR;
using VLCNotAlone.Shared.Models;
using VLCNotAlone.Shared.Models.Room;

namespace VLCNotAlone.Server.Services
{
    public class HubService : Hub
    {
        ILogger<HubService> _logger;
        MasterServerClientService _masterServerClient;
        RemoteConfigService _remoteConfigService;
        ServerAuthorizationService _serverAuthorizationService;
        RoomsService _roomsService;

        public HubService(ILogger<HubService> logger, MasterServerClientService masterServerClient, RemoteConfigService remoteConfigService, ServerAuthorizationService serverAuthorizationService, RoomsService roomsService)
        {
            _logger = logger;
            _masterServerClient = masterServerClient;
            _remoteConfigService = remoteConfigService;
            _serverAuthorizationService = serverAuthorizationService;
            _roomsService = roomsService;
        }

        public async Task<BaseHostInfo> GetHostInfo()
        {
            var hostInfo = _masterServerClient.GetBaseHostInfo();
            return hostInfo;
        }

        public async Task<bool> CheckPassword(string password)
        {
            if (!_serverAuthorizationService.TryAuthorizeUser(Context.ConnectionId, password))
            {
                throw new HubException("Invalid password");
            }

            return true;
        }

        public async Task<ListingRoomInfo[]> GetRoomsList()
        {
            if (!_serverAuthorizationService.IsUserAuthorized(Context.ConnectionId))
                throw new HubException("Not authorized");

            return _roomsService.RoomsListing;
        }
    }
}
