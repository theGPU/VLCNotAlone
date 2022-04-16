using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VLCNotAloneMultiRoomServer.Controllers;
using VLCNotAloneMultiRoomServer.Utils;

namespace VLCNotAloneMultiRoomServer
{
    internal class Program
    {
        static async Task Main()
        {
            ConfigController.Init();
            Logger.WriteLine("Main", "Starting server...");
            GetExternalIp();

            /*
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
                Logger.WriteLine("Main", $"External address: {client.GetStringAsync("http://ip-api.com/line/?fields=8192").Result.TrimEnd()}:{ConfigController.Port}");
            }
            catch
            {
                Logger.WriteLine("Main", $"External address: Error:{ConfigController.Port}");
            }
        }
    }
}
