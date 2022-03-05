using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VLCNotAlone.Controllers
{
    internal class DiscordRpcController
    {
        private static DiscordRpcClient Client;
        private static RichPresence Presence;

        private static bool _enabled = false;
        private static bool Enabled { get => _enabled; set { _enabled = value; UpdatePresence(); UpdateConfigMenuButtonsVisability(); } }

        private static void OnDiscordRPCToggle(object sender, RoutedEventArgs e) => ConfigController.ToggleDiscordRPC();

        private static void UpdateConfigMenuButtonsVisability()
        {
            MainWindow.Instance.DiscordRPCConfigActiveMenuItem.Visibility = Enabled ? Visibility.Collapsed : Visibility.Visible;
            MainWindow.Instance.DiscordRPCConfigDeactiveMenuItem.Visibility = Enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void Init()
        {
            ConfigController.OnDiscordRPCChanged += (value) => Enabled = value;

            Client = new DiscordRpcClient("949685737022976111");
            Client.Initialize();
            Presence = new RichPresence()
            {
                Assets = new Assets()
                {
                    LargeImageKey = "logo",
                    LargeImageText = "VLCNotAlone"
                },
                Buttons = new Button[]
                {
                    new Button() { Label = "Project Github", Url = "https://github.com/titaniumX712/VLCNotAlone"}
                }
            };

            App.OnApplicationExit += () => OnApplicationExit();

            MainWindow.Instance.clientApi.OnSetGlobalMediaFile += (fileName) => UpdateFileName(fileName);
            MainWindow.Instance.clientApi.OnSetLocalMediaFile += (fileName) => UpdateFileName(fileName);
            MainWindow.Instance.clientApi.OnSetInternetMediaFile += (fileName) => UpdateFileName("NetFile...");

            MainWindow.Instance.OnMediaPlayerLoaded += (mediaPlayer) => mediaPlayer.TimeChanged += (s, e) => UpdateTime(TimeSpan.FromMilliseconds(e.Time), TimeSpan.FromMilliseconds(MainWindow.Instance.currentLength));

            MainWindow.Instance.DiscordRPCConfigActiveMenuItem.Click += OnDiscordRPCToggle;
            MainWindow.Instance.DiscordRPCConfigDeactiveMenuItem.Click += OnDiscordRPCToggle;

            UpdateFileName(null, false);
            UpdateTime(null, null, false);
        }

        public static void UpdatePresence() 
        {
            if (Enabled)
                Client.SetPresence(Presence);
            else
                Client.ClearPresence();
        }

        public static void UpdateTime(TimeSpan? currentTime, TimeSpan? length, bool needUpdate = true)
        {
            //if (!currentTime.HasValue || !length.HasValue)
            //    Presence.Timestamps = null;
            //else
            //    Presence.Timestamps = new Timestamps { StartUnixMilliseconds = (ulong?)currentTime, EndUnixMilliseconds = (ulong?)length };
            if (currentTime == null || length == null)
                Presence.State = null;
            else
                Presence.State = $"{string.Format("{0:D2}:{1:D2}:{2:D2}", currentTime.Value.Hours, currentTime.Value.Minutes, currentTime.Value.Seconds)}/{string.Format("{0:D2}:{1:D2}:{2:D2}", length.Value.Hours, length.Value.Minutes, length.Value.Seconds)}";

            if (needUpdate)
                UpdatePresence();
        }

        public static void UpdateFileName(string? newFileName, bool needUpdate = true)
        {
            Presence.Details = $"Watching: {(newFileName != null ? newFileName : "Nothing")}";

            if (newFileName == null)
                UpdateTime(null, null, false);

            if (needUpdate)
                UpdatePresence();
        }

        private static void OnApplicationExit()
        {
            Client.ClearPresence();
        }
    }
}
