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
            var app = builder.Build();

            app.MapHub<HubService>("/server");

            app.Run();
        }
    }
}