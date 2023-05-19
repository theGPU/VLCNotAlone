using VLCNotAlone.Shared.Models.Configs;

namespace VLCNotAlone.Server.Services
{
    public class RemoteConfigService
    {
        ILogger<RemoteConfigService> _logger;
        
        public NetworkConfig NetworkConfig { get; private set; }
        public ushort CachedMainServerPort { get; private set; }

        public RemoteConfigService(ILogger<RemoteConfigService> logger)
        {
            _logger = logger;
            Init();
        }

        private void Init()
        {
            LoadConfigs();
        }

        private void LoadConfigs()
        {
            NetworkConfig = new NetworkConfig();
        }

        private void LoadConfig<T>(T config) where T : IRemoteConfigModel
        {

        }
    }
}
