using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;
using VLCNotAlone.Shared.Networking;

namespace VLCNotAlone.Server.Services
{
    public class MasterServerClientService
    {
        readonly ILogger<MasterServerClientService> _logger;
        RemoteConfigService _remoteConfigService;
        RoomsService _roomsService;

        MasterServerRestApiClient _client;

        public MasterServerClientService(ILogger<MasterServerClientService> logger, RemoteConfigService remoteConfigService, RoomsService roomsService)
        {
            _logger = logger;
            _remoteConfigService = remoteConfigService;
            _roomsService = roomsService;

            _client = new MasterServerRestApiClient();
        }

        public BaseHostInfo GetBaseHostInfo()
        {
            var hostInfo = new BaseHostInfo
            {
                ID = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Name = "Test server",
                Description = "Test server description",
                Port = 7133,
                Protocol = ServerProtocol.https,
                IsPublic = true,
                HasPassword = false,
                ServerVersion = Constants.AppVersion,
                ClientsCount = 0,
                RoomsCount = _roomsService.RoomsCount
            };
            return hostInfo;
        }

        private RegisterHostInfo GetRegisterHostInfo() => GetBaseHostInfo().ToRegisterHostInfo(Guid.Parse("00000000-0000-0000-0000-000000000000"));

        public async Task RegisterServer()
        {
            var registerHostInfo = GetRegisterHostInfo();
            var status = await _client.RegisterServer(registerHostInfo);
        }
    }
}
