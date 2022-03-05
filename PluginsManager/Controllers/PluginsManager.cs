using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VLCNotAlone.Plugins.Attributes;
using VLCNotAlone.Plugins.Interfaces;

namespace VLCNotAlone.Plugins.Controllers
{
    public static class PluginsManager
    {
        const string pluginsDirectory = "Plugins";

        private static List<Assembly> loadedManagedAssemblies = new List<Assembly>();
        private static List<IntPtr> loadedNativeAssemblies = new List<IntPtr>();
        private static List<IPlugin> loadedPlugins = new List<IPlugin>();

        public static T GetPlugin<T>() => loadedPlugins.OfType<T>().FirstOrDefault();

        public static void Init()
        {
            LoadPluginsDlls();
            RegisterPlugins();
        }

        internal static void RegisterPlugins()
        {
            foreach (var assembly in loadedManagedAssemblies)
                loadedPlugins.AddRange(assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t)).Select(x => (IPlugin)Activator.CreateInstance(x)).OrderBy(x => x.GetType().GetCustomAttribute<PriorityAttribute>()?.Priority ?? PriorityAttribute.Normal));

            foreach (var plugin in loadedPlugins.OfType<IInitializablePlugin>())
                plugin.Initialize();
        }

        internal static void LoadPluginsDlls()
        {
            if (!Directory.Exists(pluginsDirectory))
                Directory.CreateDirectory(pluginsDirectory);

            string[] filepaths = Directory.GetFiles(pluginsDirectory, "*.dll", SearchOption.AllDirectories);

            if (filepaths.Length == 0)
                return;

            for (int i = 0; i < filepaths.Count(); ++i)
                filepaths[i] = Path.Combine(Directory.GetCurrentDirectory(), filepaths[i]);

            var appDomain = AppDomain.CurrentDomain;
            appDomain.AssemblyResolve += AppDomain_AssemblyResolve;

            foreach (var file in filepaths)
            {
                if (IsManagedAssembly(file))
                {
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(file));
                    loadedManagedAssemblies.Add(assembly);
                }
                else
                {
                    IntPtr handle = NativeLibrary.Load(file);
                    loadedNativeAssemblies.Add(handle);
                }
            }

            foreach (var assembly in loadedManagedAssemblies)
            {
                foreach (Type pluginType in assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t)))
                    System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(pluginType.TypeHandle);
            }
        }

        private static Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // if a mod dll, resolve without version
            if (loadedManagedAssemblies.Any(assembly => assembly == args.RequestingAssembly))
            {
                var assemblyName = new AssemblyName(args.Name);
                return loadedManagedAssemblies.SingleOrDefault(assembly => assembly.GetName().Name == assemblyName.Name);
            }
            return null;
        }

        private static bool IsManagedAssembly(string fileName)
        {
            try
            {
                Stream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);
                if (fileStream.Length < 64)
                    return false;

                // PE Header starts @ 0x3C (60). Its a 4 byte header.
                fileStream.Position = 0x3C;
                uint peHeaderPointer = binaryReader.ReadUInt32();
                if (peHeaderPointer == 0)
                    peHeaderPointer = 0x80;

                // Ensure there is at least enough room for the following structures:
                //     24 byte PE Signature & Header
                //     28 byte Standard Fields         (24 bytes for PE32+)
                //     68 byte NT Fields               (88 bytes for PE32+)
                // >= 128 byte Data Dictionary Table
                if (peHeaderPointer > fileStream.Length - 256)
                    return false;

                // Check the PE signature.  Should equal 'PE\0\0'.
                fileStream.Position = peHeaderPointer;
                uint peHeaderSignature = binaryReader.ReadUInt32();
                if (peHeaderSignature != 0x00004550)
                    return false;

                // skip over the PEHeader fields
                fileStream.Position += 20;

                const ushort PE32 = 0x10b;
                const ushort PE32Plus = 0x20b;

                // Read PE magic number from Standard Fields to determine format.
                var peFormat = binaryReader.ReadUInt16();
                if (peFormat != PE32 && peFormat != PE32Plus)
                    return false;

                // Read the 15th Data Dictionary RVA field which contains the CLI header RVA.
                // When this is non-zero then the file contains CLI data otherwise not.
                ushort dataDictionaryStart = (ushort)(peHeaderPointer + (peFormat == PE32 ? 232 : 248));
                fileStream.Position = dataDictionaryStart;

                uint cliHeaderRva = binaryReader.ReadUInt32();
                if (cliHeaderRva == 0)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
