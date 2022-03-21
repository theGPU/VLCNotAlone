using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAlone.Plugins.Interfaces;

namespace VLCNotAlone.Plugins.Controllers
{
    public static class PluginsConfigController
    {
        public const string ModConfigsPath = "Configs";
        private static Dictionary<string, object> CachedConfigs = new Dictionary<string, object>();

        static PluginsConfigController()
        {
            Directory.CreateDirectory(ModConfigsPath);
        }

        public static T GetConfig<T>(this IConfigurablePlugin plugin) => GetConfig<T>(plugin.ConfigName);
        public static T GetConfig<T>(string modConfigName)
        {
            if (IsConfigExist(modConfigName))
            {
                if (CachedConfigs.ContainsKey(modConfigName))
                    return (T)CachedConfigs[modConfigName];
                else
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(Path.Combine(ModConfigsPath, modConfigName)));
            } else
            {
                var config = Activator.CreateInstance(typeof(T));
                UpdateConfig(modConfigName, config);
                return (T)config;
            }
        }

        public static void UpdateConfig(this IConfigurablePlugin plugin, object config) => UpdateConfig(plugin.ConfigName, config);
        public static void UpdateConfig(string modConfigName, object config)
        {
            if (!CachedConfigs.ContainsKey(modConfigName))
                CachedConfigs.Add(modConfigName, config);
            else if (CachedConfigs[modConfigName] != config)
                CachedConfigs[modConfigName] = config;

            File.WriteAllText(Path.Combine(ModConfigsPath, modConfigName), JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public static bool IsConfigExist(string modConfigName) => File.Exists(Path.Combine(ModConfigsPath, modConfigName));
    }
}
