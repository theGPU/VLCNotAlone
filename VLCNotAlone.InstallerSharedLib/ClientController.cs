using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAlone.InstallerShared
{
    public static class ClientController
    {
        private const string ClientFileName = "VLCNotAlone.exe";

        public static bool CheckClientInFolder(string path)
        {
            if (File.Exists(Path.Combine(path, ClientFileName)))
                return true;
            return false;
        }

        public static string? GetClientVersion(string path)
        {
            var clientFilePath = Path.Combine(path, ClientFileName);
            if (File.Exists(clientFilePath))
                return FileVersionInfo.GetVersionInfo(clientFilePath).ProductVersion!;
            else
                return null;
        }

        public static bool CheckServersFileExist(string path) => File.Exists(Path.Combine(path, "Servers.txt"));
        public static bool CheckVLCFolderExist(string path) => Directory.Exists(Path.Combine(path, "libvlc"));

        public static void UnpackZipUpdate(ZipArchive update, string path, Action<double> progressCallback)
        {
            progressCallback.Invoke(0);

            var inUpdateFolderName = $"VLCNotAlone win-{(Environment.Is64BitOperatingSystem ? "x64" : "x86")}/";
            var filesToExtract = update.Entries.Where(x => !x.FullName.EndsWith('/')).ToArray();
            if (CheckClientInFolder(path))
                filesToExtract = filesToExtract.Where(x => x.Name != "Servers.txt").ToArray();
            if (CheckVLCFolderExist(path))
                filesToExtract = filesToExtract.Where(x => !x.FullName.Contains("/libvlc/")).ToArray();

            for (var i = 0; i < filesToExtract.Length; i++)
            {
                var file = filesToExtract[i];
                var newFilePath = Path.Combine(path, file.FullName.Replace(inUpdateFolderName, ""));
                if (!Directory.Exists(Path.GetDirectoryName(newFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                file.ExtractToFile(newFilePath, true);
                progressCallback.Invoke((double) i / filesToExtract.Length);
            }
            update.Dispose();
        }

        public static byte[] UnpackInstaller(ZipArchive update)
        {
            var installerEntry = update.Entries.First(x => x.Name == "VLCNotAlone.Installer.exe");
            using var stream = installerEntry.Open();
            using var streamReader = new BinaryReader(stream);
            return streamReader.ReadBytes((int)installerEntry.Length);
        }
    }
}
