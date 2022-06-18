using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAlone.Installer.Controllers
{
    internal static class ConfigController
    {
        const string ConfigPath = "Config.json";
        private static ConfigPOCO config;

        public static Action OnConfigControllerInited;

        public static void Init()
        {
            config = File.Exists(ConfigPath) ? JsonConvert.DeserializeObject<ConfigPOCO>(File.ReadAllText(ConfigPath), new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate })! : new ConfigPOCO();
            SaveConfig();

            OnConfigControllerInited?.Invoke();
        }

        private static void SaveConfig() => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));

        public static string Language { get => config.Language; set { config.Language = value; SaveConfig(); } }
        public static bool AutoUpdateInstaller { get => config.AutoUpdateInstaller; set { config.AutoUpdateInstaller = value; SaveConfig(); } }
        public static bool RememberLastSelectedPath { get => config.RememberLastSelectedPath; set { config.RememberLastSelectedPath = value; SaveConfig(); } }
        public static string LastSelectedPath { get => config.LastSelectedPath; set { config.LastSelectedPath = value; SaveConfig(); } }
    }

    internal class ConfigPOCO
    {
        [DefaultValue("en_US")] public string Language { get; set; } = "en_US";
        [DefaultValue(true)] public bool AutoUpdateInstaller { get; set; } = true;
        [DefaultValue(true)] public bool RememberLastSelectedPath { get; set; } = true;
        [DefaultValue("")] public string LastSelectedPath { get; set; } = "";
    }
}
