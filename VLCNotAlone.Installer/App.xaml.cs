using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace VLCNotAlone.Installer
{
    public partial class App : Application
    {
        private void Main(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();
            if (e.Args.Length == 1)
                CopyToOriginalFolder(e.Args[0]);
            wnd.Show();
        }

        private static void CopyToOriginalFolder(string path)
        {
            Task.Delay(5000);
            var currentFilePath = Environment.GetCommandLineArgs()[0];
            File.Copy(currentFilePath, path, true);
        }
    }
}
