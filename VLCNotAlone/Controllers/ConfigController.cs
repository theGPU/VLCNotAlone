using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public static Action<uint> OnFileCachingTimeChanged;
        public static Action<uint> OnNetworkCachingTimeChanged;
        public static Action<bool> OnDiscordRPCChanged;

        public static void Init()
        {
            if (File.Exists(ConfigPath))
            {
                config = JsonConvert.DeserializeObject<ConfigPOCO>(File.ReadAllText(ConfigPath))!;
            } else
            {
                config = new ConfigPOCO();
                SaveConfig();
            }

            OnFileCachingTimeChanged?.Invoke(config.FileCachingTime);
            OnNetworkCachingTimeChanged?.Invoke(config.NetworkCachingTime);
            OnDiscordRPCChanged?.Invoke(config.DiscordRPC);
        }

        private static void SaveConfig() => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));

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

        public static void ToggleDiscordRPC()
        {
            config.DiscordRPC = !config.DiscordRPC;
            OnDiscordRPCChanged?.Invoke(config.DiscordRPC);
            SaveConfig();
        }
    }

    internal class ConfigPOCO
    {
        public uint FileCachingTime { get; set; } = 5000;
        public uint NetworkCachingTime { get; set; } = 5000;
        public bool DiscordRPC { get; set; } = true;
    }
}
