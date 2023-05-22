using Microsoft.AspNetCore.SignalR;
using VLCNotAlone.Server.Services;

namespace VLCNotAlone.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<RemoteConfigService>();
            builder.Services.AddSingleton<ServerAuthorizationService>();
            builder.Services.AddSingleton<RoomsService>();

            builder.Services.AddSingleton<MasterServerClientService>();
            builder.Services.AddHostedService<MasterServerRegisterService>();
            var app = builder.Build();

            app.MapHub<HubService>("/server");

            app.Run();
        }
    }
}