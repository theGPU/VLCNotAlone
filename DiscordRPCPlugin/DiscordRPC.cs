using DiscordRPC;
using LibVLCSharp.Shared;
using System;
using System.Windows;
using System.Windows.Controls;
using VLCNotAlone;
using VLCNotAlone.Plugins;
using VLCNotAlone.Plugins.Controllers;
using VLCNotAlone.Plugins.Interfaces;
using Button = DiscordRPC.Button;

namespace DiscordRPCPlugin
{
    public class DiscordRPC : IInitializablePlugin, IConfigurablePlugin
    {
        public string Name => "DiscordRPC";
        public string Description => "Sample plugin";
        public string ConfigName => "DiscordRPC.json";

        private DiscordRpcClient Client;
        private RichPresence Presence;

        private DiscordRPCConfig Config;

        private MenuItem PluginActiveButton;
        private MenuItem PluginDeactiveButton;

        public void Initialize()
        {
            Config = this.GetConfig<DiscordRPCConfig>();

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

            MainWindow.Instance.OnMediaPlayerLoaded += (MediaPlayer mediaPlayer) =>
            {
                mediaPlayer.LengthChanged += (s, e) => UpdateTime(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(e.Length));
                mediaPlayer.TimeChanged += (s, e) => UpdateTime(TimeSpan.FromMilliseconds(e.Time), TimeSpan.FromMilliseconds(MainWindow.Instance.currentLength));
            };

            CreateClientMenuButtons();

            UpdateFileName(null, false);
            UpdateTime(null, null, false);
            UpdatePresence();
        }

        public void CreateClientMenuButtons()
        {
            PluginActiveButton = new MenuItem { Header = "Active" };
            PluginActiveButton.Click += TogglePlugin;

            PluginDeactiveButton = new MenuItem { Header = "Deactive" };
            PluginDeactiveButton.Click += TogglePlugin;

            var pluginMenu = new MenuItem { Header = nameof(DiscordRPC) };
            pluginMenu.Items.Add(PluginActiveButton);
            pluginMenu.Items.Add(PluginDeactiveButton);

            MainWindow.PluginsClientMenu.Items.Add(pluginMenu);
            UpdateActiveButtonsVisability();
        }

        private void TogglePlugin(object sender, RoutedEventArgs e)
        {
            Config.Enabled = !Config.Enabled;
            UpdateActiveButtonsVisability();
            UpdatePresence();
            this.UpdateConfig(Config);
        }

        private void UpdateActiveButtonsVisability()
        {
            PluginActiveButton.Visibility = Config.Enabled ? Visibility.Collapsed : Visibility.Visible;
            PluginDeactiveButton.Visibility = Config.Enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        public void UpdatePresence()
        {
            if (Config.Enabled)
                Client.SetPresence(Presence);
            else
                Client.ClearPresence();
        }

        public void UpdateTime(TimeSpan? currentTime, TimeSpan? length, bool needUpdate = true)
        {
            if (currentTime == null || length == null)
                Presence.State = null;
            else
                Presence.State = $"{string.Format("{0:D2}:{1:D2}:{2:D2}", currentTime.Value.Hours, currentTime.Value.Minutes, currentTime.Value.Seconds)}/{string.Format("{0:D2}:{1:D2}:{2:D2}", length.Value.Hours, length.Value.Minutes, length.Value.Seconds)}";

            if (needUpdate)
                UpdatePresence();
        }

        public void UpdateFileName(string? newFileName, bool needUpdate = true)
        {
            Presence.Details = $"Watching: {(newFileName != null ? newFileName : "Nothing")}";

            if (newFileName == null)
                UpdateTime(null, null, false);

            if (needUpdate)
                UpdatePresence();
        }

        private void OnApplicationExit()
        {
            Client.ClearPresence();
        }
    }
}
