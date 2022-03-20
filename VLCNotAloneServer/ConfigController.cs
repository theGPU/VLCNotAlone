using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAloneServer
{
    internal class ConfigController
    {
        const string ConfigPath = "Config.json";
        private static ConfigPOCO config;

        public static void Init()
        {
            if (File.Exists(ConfigPath))
            {
                config = JsonConvert.DeserializeObject<ConfigPOCO>(File.ReadAllText(ConfigPath), new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Populate })!;
                SaveConfig();
            }
            else
            {
                config = new ConfigPOCO();
                SaveConfig();
            }
        }

        private static void SaveConfig() => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(config, Formatting.Indented));

        public static ushort Port { get => config.Port; set { config.Port = value; SaveConfig(); } }
    }

    internal class ConfigPOCO
    {
        [DefaultValue(4096)]
        public ushort Port = 4096;
    }
}
