using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAloneMultiRoomServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ConfigController.Init();
            Console.WriteLine("Starting server...");
            GetExternalIp();

            /*
            ServerListenerController.OnServerStarted += () => Console.WriteLine("Server started");
            ServerListenerController.OnMessageRecived += (sender, message, metadata) => Console.WriteLine($"Server recived new message: \"{message}\" from \"{sender}\"");
            ServerListenerController.OnClientConnected += (client) => Console.WriteLine($"New client connected: {client}");
            ServerListenerController.OnClientConnected += (client) => ServerListenerController.SendMessage(client, "this is a test message");
            ServerListenerController.StartServer(null, ConfigController.Port);

            var client = new DebugClient("localhost", ConfigController.Port);
            DebugClient.OnMessageRecived = (client, message, metadata) => {
                Console.WriteLine($"Client recived message: \"{message}\"");
                client.SendMessage(message);
            };
            client.Connect();
            */

            while (true)
                await Task.Delay(1000);
        }

        static void GetExternalIp()
        {
            try
            {
                using HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(1);
                Console.WriteLine($"External address: {client.GetStringAsync("http://ip-api.com/line/?fields=8192").Result.TrimEnd()}:{ConfigController.Port}");
            }
            catch
            {
                Console.WriteLine($"External address: Error:{ConfigController.Port}");
            }
        }
    }
}
