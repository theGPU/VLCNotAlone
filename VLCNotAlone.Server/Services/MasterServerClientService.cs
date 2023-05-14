﻿using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.Server.Services
{
    public class MasterServerClientService : BackgroundService
    {
        readonly ILogger<MasterServerClientService> _logger;
        RemoteConfigService _remoteConfigService;

        public MasterServerClientService(ILogger<MasterServerClientService> logger, RemoteConfigService remoteConfigService)
        {
            _logger = logger;
            _remoteConfigService = remoteConfigService;
        }

        private BaseHostInfo GetBaseHostInfo()
        {
            var hostInfo = new BaseHostInfo
            {
                ID = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                Name = "Test server",
                Description = "Test server description",
                Port = 7133,
                IsPublic = true,
                HasPassword = true,
                ServerVersion = Constants.AppVersion,
                ClientsCount = 0,
                RoomsCount = 0
            };
            return hostInfo;
        }

        private BaseHostInfo GetRegisterHostInfo() => GetBaseHostInfo().ToRegisterHostInfo(Guid.Parse("00000000-0000-0000-0000-000000000000"));

        private async Task RegisterServer()
        {
            var registerHostInfo = GetRegisterHostInfo();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                do
                {
                    await RegisterServer();
                    await Task.Delay(TimeSpan.FromSeconds(Constants.MasterServerRefreshTimeSeconds), stoppingToken);
                }
                while (!stoppingToken.IsCancellationRequested);
            }
            catch (Exception ex) { _logger.LogError(ex, "Failed to execute server registration task"); }
        }
    }
}
