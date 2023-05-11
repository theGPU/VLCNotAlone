
using VLCNotAlone.MasterServer.Services;
using VLCNotAlone.MasterServer.Services.ServerRegistrar;
using VLCNotAlone.MasterServer.Services.ServerRegistry;

namespace VLCNotAlone.MasterServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddApiVersioning();
            builder.Services.AddHealthChecks();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<Program>>();
            builder.Services.AddSingleton(typeof(ILogger), logger);

            builder.Services.AddSingleton<IServerRegistry, InMemoryServerRegistry>();
            builder.Services.AddSingleton<IServerRegistrar, ServerRegistrar>();
            builder.Services.AddSingleton<ServerBanManager>();

            builder.Services.AddHostedService<ServerRegistryCleanupBackgroundService>();
            builder.Services.AddHostedService<ServerRegistryInitializer>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}