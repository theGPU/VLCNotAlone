using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using VLCNotAloneMultiRoomServer.Controllers;
using VLCNotAloneMultiRoomServer.Utils;
using VLCNotAloneShared;

namespace VLCNotAloneMultiRoomServer
{
    internal class Programs
    {
        static async Task Main()
        {
            ConfigController.Init();
            Logger.WriteLine("Main", $"Starting VLCNotAloneMultiRoomServer v{VersionInfo.Version} with API version: {VersionInfo.ApiVersion}...");
            GetExternalIp();

            RoomsController.ReloadRooms();
            var roomsRefreshTimer = new Timer() { Interval = TimeSpan.FromSeconds(10).TotalMilliseconds, AutoReset = true };
            roomsRefreshTimer.Elapsed += (s, e) => RoomsController.ReloadRooms();
            roomsRefreshTimer.Start();

            ServerController.Init();

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
