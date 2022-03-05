using DiscordRPC;
using System;
using VLCNotAlone;
using VLCNotAlone.Plugins;
using VLCNotAlone.Plugins.Interfaces;

namespace DiscordRPCPlugin
{
    public class DiscordRPC : IInitializablePlugin
    {
        public string Name => "DiscordRPC";
        public string Description => "Sample plugin";

        private static DiscordRpcClient Client;
        private static RichPresence Presence;

        public void Initialize()
        {
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

            //MainWindow.Instance.DiscordRPCConfigActiveMenuItem.Click += OnDiscordRPCToggle;
            //MainWindow.Instance.DiscordRPCConfigDeactiveMenuItem.Click += OnDiscordRPCToggle;

            UpdateFileName(null, false);
            UpdateTime(null, null, false);
        }

        public static void UpdatePresence()
        {
            if (true)
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
