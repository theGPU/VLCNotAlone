using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VLCNotAlone.Utils;
using VLCNotAloneShared;

namespace VLCNotAlone.HelpPages
{
    /// <summary>
    /// Логика взаимодействия для AboutProgramWindow.xaml
    /// </summary>
    public partial class AboutProgramWindow : Window
    {
        public AboutProgramWindow()
        {
            InitializeComponent();
            this.Title = Localizer.DoStr(this.Title);
            this.VersionLabel.Content = Localizer.Do($"Version: {VersionInfo.Version} API: {VersionInfo.ApiVersion}");
        }

        private void OnClickOpenGitHubPage(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/titaniumX712/VLCNotAlone",
                UseShellExecute = true
            });
        }
    }
}
