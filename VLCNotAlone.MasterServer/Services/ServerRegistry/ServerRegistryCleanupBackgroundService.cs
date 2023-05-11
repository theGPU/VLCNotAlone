namespace VLCNotAlone.MasterServer.Services.ServerRegistry
{
    public class ServerRegistryCleanupBackgroundService : BackgroundService
    {
        readonly ILogger<ServerRegistryCleanupBackgroundService> _logger;
        readonly IServerRegistry _serverRegistry;

        public ServerRegistryCleanupBackgroundService(ILogger<ServerRegistryCleanupBackgroundService> logger, IServerRegistry serverRegistry)
        {
            _logger = logger;
            _serverRegistry = serverRegistry;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                do
                {
                    var now = DateTime.Now;
                    _serverRegistry.CleanupServerRegistry(now);
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
                while (!stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to execute world shutdown background task."); }
        }
    }
}
