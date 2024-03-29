﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAlone.Controllers
{
    internal static class ConfigController
    {
        const string ConfigPath = "Config.json";
        private static ConfigPOCO config;

        public static Action OnConfigControllerInited;
        public static Action<uint> OnFileCachingTimeChanged;
        public static Action<uint> OnNetworkCachingTimeChanged;

        public static void Init()
        {
            config = File.Exists(ConfigPath) ? JsonConvert.DeserializeObject<ConfigPOCO>(File.ReadAllText(ConfigPath), new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate })! : new ConfigPOCO();
            SaveConfig();

            OnFileCachingTimeChanged?.Invoke(config.FileCachingTime);
            OnNetworkCachingTimeChanged?.Invoke(config.NetworkCachingTime);

            OnConfigControllerInited?.Invoke();
        }

        private static void SaveConfig() => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));

        public static bool ShowLogo { get => config.ShowLogo; set { config.ShowLogo = value; SaveConfig(); } }
        public static string Language { get => config.Language; set { config.Language = value; SaveConfig(); } }

        public static void SetFileCachingTime(uint newCacheTime)
        {
            if (config.FileCachingTime == newCacheTime)
                return;

            config.FileCachingTime = newCacheTime;
            OnFileCachingTimeChanged?.Invoke(newCacheTime);
            SaveConfig();
        }

        public static void SetNetworkCachingTime(uint newCacheTime)
        {
            if (config.NetworkCachingTime == newCacheTime)
                return;

            config.NetworkCachingTime = newCacheTime;
            OnNetworkCachingTimeChanged?.Invoke(newCacheTime);
            SaveConfig();
        }

        public static string Nickname { get => config.Nickname; set { config.Nickname = value; SaveConfig(); } }

        public static bool CheckForUpdatesAtStartup { get => config.CheckForUpdatesAtStartup; set { config.CheckForUpdatesAtStartup = value; SaveConfig(); } }

        public static bool ConnectToDefaultRoomAutomatically { get => config.ConnectToDefaultRoomAutomatically; set { config.ConnectToDefaultRoomAutomatically = value; SaveConfig(); } }
    }

    internal class ConfigPOCO
    {
        [DefaultValue(true)] public bool ShowLogo { get; set; } = true;
        [DefaultValue("en_US")] public string Language { get; set; } = "en_US";
        [DefaultValue(true)] public bool CheckForUpdatesAtStartup { get; set; } = true;
        [DefaultValue(true)] public bool ConnectToDefaultRoomAutomatically { get; set; } = true;
        [DefaultValue("")] public string Nickname { get; set; } = "";
        [DefaultValue(5000)] public uint FileCachingTime { get; set; } = 5000;
        [DefaultValue(5000)] public uint NetworkCachingTime { get; set; } = 5000;
    }
}
