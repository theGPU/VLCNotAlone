namespace VLCNotAlone.MasterServer.Services.ServerRegistry
{
    public class ServerRegistryInitializer : IHostedService
    {
        readonly IServerRegistry _serverRegistry;

        public ServerRegistryInitializer(
            IServerRegistry serverRegistry,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _serverRegistry = serverRegistry;
            hostApplicationLifetime.ApplicationStopping.Register(this.ShutdownServerRegistry);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _serverRegistry.Initialize();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        void ShutdownServerRegistry() => _serverRegistry.Shutdown();
    }
}
