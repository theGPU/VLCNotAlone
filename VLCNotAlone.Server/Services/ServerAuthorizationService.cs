namespace VLCNotAlone.Server.Services
{
    public class ServerAuthorizationService
    {
        private ILogger<ServerAuthorizationService> _logger;
        private RemoteConfigService _remoteConfigService;

        public ServerAuthorizationService(ILogger<ServerAuthorizationService> logger, RemoteConfigService remoteConfigService)
        {
            _logger = logger;
            _remoteConfigService = remoteConfigService;
        }

        private readonly HashSet<string> authorizedUsers = new();

        public bool IsUserAuthorized(string userConnectionId) => authorizedUsers.Contains(userConnectionId);

        public bool TryAuthorizeUser(string userConnectionId, string password)
        {
            if (!string.IsNullOrEmpty(_remoteConfigService.NetworkConfig.Password) && _remoteConfigService.NetworkConfig.Password != password)
            {
                return false;
            }

            authorizedUsers.Add(userConnectionId);
            return true;
        }
    }
}
