using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VLCNotAlone.InstallerShared;

namespace VLCNotAlone.AutoUpdater
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                OnFirstRun();
            else
            {
                if (args[0] == "update")
                    OnUpdate();
                else if (args[0] == "clear")
                    OnClearTempDirectory(false);
                else if (args[0] == "clearAndRun")
                    OnClearTempDirectory(true);
            } 
        }

        public static void OnFirstRun()
        {
            Console.WriteLine("Welcome to auto updater");
            Console.WriteLine("Copying updater to temp folder...");
            var currentAssemblyPath = Environment.GetCommandLineArgs()[0];
            if (!Directory.Exists("./temp"))
                Directory.CreateDirectory("./temp");
            File.Copy(currentAssemblyPath, "./temp/Updater.exe", true);

            var args = new string[] { "update" };
            var pst = new ProcessStartInfo("./temp/Updater.exe");
            pst.Arguments = string.Join(" ", args.Where(s => !string.IsNullOrEmpty(s)).Select(it => ("\"" + Regex.Replace(it, @"(\\+)$", @"$1$1") + "\""))); ;

            Process.Start(pst);
            Process.GetCurrentProcess().Kill();
        }

        public static void OnUpdate()
        {
            Console.WriteLine("Welcome to auto updater");
            Console.WriteLine("Waiting...");
            Task.Delay(5000).Wait();
            var clientVersion = ClientController.GetClientVersion("..");
            clientVersion = clientVersion == null ? "None" : clientVersion == "1.0.0" ? "Unknown" : clientVersion;
            Console.WriteLine($"Installed client version: {clientVersion}");
            Console.WriteLine("Checking for updates...");
            var lastTag = GitHubController.GetTags().Where(x => !x.StartsWith("v1")).LastOrDefault();
            if (lastTag == null)
            {
                Console.WriteLine("Tag not found. Rate limit?");
                RunInClearMode();
            }
            Console.WriteLine($"Latest version: {lastTag}");
            if (clientVersion != lastTag)
            {
                Console.WriteLine("Installing latest version...");
                var progressBarOptions = new ProgressBarOptions { ForegroundColor = ConsoleColor.Green, ForegroundColorDone = ConsoleColor.DarkGreen, BackgroundColor = ConsoleColor.DarkGray, BackgroundCharacter = '\u2593' };
                var progressBar = new ProgressBar(10000, $"Downloading update archive...", progressBarOptions);
                IProgress<double> progressBarProgress = progressBar.AsProgress<double>();
                var updateArchive = GitHubController.DownloadRelease(lastTag, (progress) => progressBarProgress.Report(progress));
                progressBar.Message = "Unpacking update...";
                progressBarProgress.Report(0);
                ClientController.UnpackZipUpdate(updateArchive, "./", (progress) => progressBarProgress.Report(progress));
                progressBar.Message = "Complete";
                GitHubController.CloseStreams();
                progressBar.Dispose();
                Console.WriteLine("Restarting...");

                var args = new string[] { "clearAndRun" };
                var pst = new ProcessStartInfo("./Updater.exe");
                pst.Arguments = string.Join(" ", args.Where(s => !string.IsNullOrEmpty(s)).Select(it => ("\"" + Regex.Replace(it, @"(\\+)$", @"$1$1") + "\""))); ;

                Process.Start(pst);
                Process.GetCurrentProcess().Kill();
            }
        }

        public static void RunInClearMode()
        {
            var args = new string[] { "clear" };
            var pst = new ProcessStartInfo("./Updater.exe");
            pst.Arguments = string.Join(" ", args.Where(s => !string.IsNullOrEmpty(s)).Select(it => ("\"" + Regex.Replace(it, @"(\\+)$", @"$1$1") + "\""))); ;

            Process.Start(pst);
            Process.GetCurrentProcess().Kill();
        }

        public static void OnClearTempDirectory(bool runClient)
        {
            Console.WriteLine("Welcome to auto updater");
            Console.WriteLine("Waiting...");
            Task.Delay(5000).Wait();
            Console.WriteLine("Clearing temp directory...");
            ClearTempDir();
            if (runClient)
            {
                Console.WriteLine("Complete.");
                Console.WriteLine("Starting client...");
                Process.Start("./VLCNotAlone.exe");
            }
            else
            {
                Console.WriteLine("Complete. Bye!");
            }
            Process.GetCurrentProcess().Kill();
        }

        public static void ClearTempDir()
        {
            if (Directory.Exists("./temp"))
                Directory.Delete("./temp", true);
        }
    }
}
