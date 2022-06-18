using Newtonsoft.Json;
using ICSharpCode.SharpZipLib.Zip;
using System.IO.Compression;
using System.Text.Json;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using System;
using System.Linq;

namespace VLCNotAlone.InstallerShared
{
    public static class GitHubController
    {
        private static MemoryStream? DownloadStream;

        private const string RepoName = "titaniumX712/VLCNotAlone";
        public static string[] GetTags()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "VLCNotAlone Installer");

            try
            {
                return JsonConvert.DeserializeObject<IEnumerable<dynamic>>(client.GetStringAsync($"https://api.github.com/repos/{RepoName}/git/refs/tags").Result).Select(x => ((string)x.@ref).Replace("refs/tags/", "")).ToArray();
            } catch (Exception ex)
            {
                return new string[0];
            }
        }

        public static ZipArchive DownloadRelease(string tag, Action<double> progressCallback)
        {
            progressCallback.Invoke(0);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "VLCNotAlone Installer");

            var release = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(client.GetStringAsync($"https://api.github.com/repos/{RepoName}/releases").Result).First(x => ((string)x.tag_name) == tag);
            var architectureSuffix = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            var asset = (release.assets as IEnumerable<dynamic>).First(x => ((string)x.name).StartsWith("VLCNotAlone") && ((string)x.name).EndsWith($"{architectureSuffix}.zip"));
            DownloadFile((string)asset.browser_download_url, progressCallback);
            return new ZipArchive(DownloadStream!);
        }

        public static ZipArchive? CheckInstallerUpdate(string currentVersion, Action<double> progressCallback)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "VLCNotAlone Installer");

            var lastTag = GetTags().LastOrDefault();
            if (lastTag == null)
                return null;

            var release = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(client.GetStringAsync($"https://api.github.com/repos/{RepoName}/releases").Result).First(x => ((string)x.tag_name) == lastTag);
            var architectureSuffix = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            var asset = (release.assets as IEnumerable<dynamic>).FirstOrDefault(x => ((string)x.name).StartsWith("VLCNotAlone.Installer") && ((string)x.name).EndsWith($"{architectureSuffix}.zip"));
            if (asset == null)
                return null;
            if (!((string)asset.name).Contains(currentVersion))
            {
                DownloadFile((string)asset.browser_download_url, progressCallback);
                return new ZipArchive(DownloadStream!);
            }
            return null;
        }

        public static void DownloadFile(string url, Action<double> progressCallback)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "VLCNotAlone Installer");

            using var response = client.GetAsync(url).Result;
            using var tempDownloadingStream = response.Content.ReadAsStreamAsync().Result;
            var totalBytes = (int)response.Content.Headers.ContentLength!;
            var bytesRecived = 0;
            var buffer = new byte[1024];
            DownloadStream = new MemoryStream(totalBytes);
            while (totalBytes != bytesRecived)
            {
                var currentCycleBytesRead = tempDownloadingStream.Read(buffer, 0, 1024);
                DownloadStream.Write(buffer, 0, currentCycleBytesRead);
                bytesRecived += currentCycleBytesRead;
                progressCallback.Invoke((double)bytesRecived / totalBytes);
            }
        }

        public static void CloseStreams()
        {
            if (DownloadStream != null)
            {
                DownloadStream.Dispose();
                DownloadStream = null;
            }
        }
    }
}