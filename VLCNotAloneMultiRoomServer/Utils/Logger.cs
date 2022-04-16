using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VLCNotAloneMultiRoomServer.Utils
{
    internal static class Logger
    {
        static readonly object locker = new object();
        static readonly string LogFilePatch;

        static Logger()
        {
            Directory.CreateDirectory("./Logs");
            LogFilePatch = Path.Combine("./Logs",DateTime.Now.ToString($"{GetCurrentLogTime()}.log"));
        }

        static string GetCurrentLogTime() => DateTime.Now.ToString("[yy-MM-dd HH:mm:ss]");

        public static void WriteLine(string module, string msg)
        {
            var logLine = $"{GetCurrentLogTime()} [{module}]: {msg}{Environment.NewLine}";
            lock (locker)
                File.AppendAllText(LogFilePatch, logLine);
            Console.WriteLine(logLine);
        }
    }
}
