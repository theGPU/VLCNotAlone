using Microsoft.AspNetCore.SignalR;

namespace VLCNotAlone.Server.Services
{
    public class HubService : Hub
    {
        ILogger<HubService> _logger;
        MasterServerClientService _masterServerClient;
        RemoteConfigService _remoteConfigService;

        public HubService(ILogger<HubService> logger, MasterServerClientService masterServerClient, RemoteConfigService remoteConfigService)
        {
            _logger = logger;
            _masterServerClient = masterServerClient;
            _remoteConfigService = remoteConfigService;
        }

        public async Task Send(string username, string message)
        {
            await this.Clients.All.SendAsync("Receive", username, message);
        }
    }
}
