using VLCNotAlone.Shared;

namespace VLCNotAlone.Server.Services
{
    public class MasterServerRegisterService : BackgroundService
    {
        ILogger<RemoteConfigService> _logger;
        MasterServerClientService _masterServerClient;

        public MasterServerRegisterService(ILogger<RemoteConfigService> logger, MasterServerClientService masterServerClient)
        {
            _logger = logger;
            _masterServerClient = masterServerClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                do
                {
                    await _masterServerClient.RegisterServer();
                    await Task.Delay(TimeSpan.FromSeconds(Constants.MasterServerRefreshTimeSeconds), stoppingToken);
                }
                while (!stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to execute server registration task"); }
        }
    }
}
