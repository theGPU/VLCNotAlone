using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VLCNotAlone.Installer.Controllers;
using VLCNotAlone.Installer.Utils;
using VLCNotAlone.InstallerShared;

namespace VLCNotAlone.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        private string? InstalledVersion = null;
        private string? SelectedVersion => VersionSelector.SelectedItem as string;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            ConfigController.Init();
            Localizer.Init();

            VersionSelector.ItemsSource = GitHubController.GetTags().Where(x => !x.StartsWith("v1")).Reverse();
            VersionSelector.SelectedIndex = 0;

            PrepareSettingsCheckBoxes();
            VersionLabel.Content = Localizer.Do($"Installer v{InstallerInfo.Version}");

            if (ConfigController.RememberLastSelectedPath && Directory.Exists(ConfigController.LastSelectedPath))
                InstallationPathTextBlock.Text = ConfigController.LastSelectedPath;

            //ConfigController.AutoUpdateInstaller = false;
            //AutoUpdateInstallerCheckBox.IsChecked = ConfigController.AutoUpdateInstaller;

            if (ConfigController.AutoUpdateInstaller)
                CheckInstallerUpdate();
        }

        private void CheckInstallerUpdate()
        {
            OnUpdateUpdateProgress($"Checking installer update...");
            this.Dispatcher.Invoke(() => { InstallButton.IsEnabled = false; });
            var updateZip = GitHubController.CheckInstallerUpdate(InstallerInfo.Version, (progress) => OnUpdateUpdateProgress($"Updating installer... {progress * 100:0.00}%"));
            if (updateZip != null)
            {
                var installerFile = ClientController.UnpackInstaller(updateZip);
                var tempFile = System.IO.Path.GetTempFileName();
                File.WriteAllBytes(tempFile, installerFile);
                var currentFilePath = Environment.GetCommandLineArgs()[0];

                var args = new string[] { currentFilePath };
                var pst = new ProcessStartInfo(tempFile);
                pst.Arguments = string.Join(" ", args.Where(s => !string.IsNullOrEmpty(s)).Select(it => ("\"" + Regex.Replace(it, @"(\\+)$", @"$1$1") + "\""))); ;

                Process.Start(pst);
                Process.GetCurrentProcess().Kill();
            }
            this.Dispatcher.Invoke(() => { UpdateInstallButtonText(); InstallButton.IsEnabled = true; });
        }

        private void PrepareSettingsCheckBoxes()
        {
            void OnToggleAutoUpdate()
            {
                ConfigController.AutoUpdateInstaller = !ConfigController.AutoUpdateInstaller;
                AutoUpdateInstallerCheckBox.IsChecked = ConfigController.AutoUpdateInstaller;
            }

            void OnToggleRememberLastPath()
            {
                ConfigController.RememberLastSelectedPath = !ConfigController.RememberLastSelectedPath;
                RememberLastSelectedPathCheckBox.IsChecked = ConfigController.RememberLastSelectedPath;
            }

            AutoUpdateInstallerCheckBox.IsChecked = ConfigController.AutoUpdateInstaller;
            AutoUpdateInstallerCheckBox.Checked += (s, e) => OnToggleAutoUpdate();
            AutoUpdateInstallerCheckBox.Unchecked += (s, e) => OnToggleAutoUpdate();

            RememberLastSelectedPathCheckBox.IsChecked = ConfigController.RememberLastSelectedPath;
            RememberLastSelectedPathCheckBox.Checked += (s, e) => OnToggleRememberLastPath();
            RememberLastSelectedPathCheckBox.Unchecked += (s, e) => OnToggleRememberLastPath();
        }

        public void OnAnotherLanguageSelected(object sender, RoutedEventArgs e)
        {
            ConfigController.Language = (string)LanguageSelector.SelectedItem;
            Localizer.OnLanguageChanged();
        }

        private void OnClickExit(object sender, MouseEventArgs e)
        {
            this.Close();
        }

        private void OnOpenGithubPage(object sender, MouseButtonEventArgs e) => System.Diagnostics.Process.Start(new ProcessStartInfo {FileName = "https://github.com/titaniumX712/VLCNotAlone", UseShellExecute = true});

        private void OnDragWindow(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void OnSelectInstallPath(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folderPickerDialog = new CommonOpenFileDialog();
            folderPickerDialog.IsFolderPicker = true;
            if (folderPickerDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var selectedPath = folderPickerDialog.FileName;
                InstallationPathTextBlock.Text = selectedPath;
                if (ConfigController.RememberLastSelectedPath)
                    ConfigController.LastSelectedPath = selectedPath;
                UpdateInstallButtonText();
                InstallButton.IsEnabled = true;
            }
        }

        private void UpdateInstalledVersion()
        {
            var selectedPath = InstallationPathTextBlock.Text;
            if (ClientController.CheckClientInFolder(selectedPath))
                InstalledVersion = ClientController.GetClientVersion(selectedPath);
            else
                InstalledVersion = null;
        }

        private void UpdateInstallButtonText()
        {
            UpdateInstalledVersion();
            if (InstalledVersion == null)
                InstallButton.Content = Localizer.DoStr("Install");
            else if (InstalledVersion == SelectedVersion)
                InstallButton.Content = Localizer.DoStr("Reinstall");
            else if (InstalledVersion == "1.0.0")
                InstallButton.Content = Localizer.DoStr("Update / Reinstall");
            else if (String.Compare(InstalledVersion, SelectedVersion) < 0)
                InstallButton.Content = Localizer.DoStr("Update");
            else
                InstallButton.Content = Localizer.DoStr("Downgrade");
        }

        private void OnAnotherVersionSelected(object sender, RoutedEventArgs e) => UpdateInstallButtonText();

        private void OnLatestCheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                VersionSelector.IsEnabled = false;
                VersionSelector.SelectedIndex = 0;
            }
            else
            {
                VersionSelector.IsEnabled = true;
            }
        }

        private void OnInstallButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedVersion = SelectedVersion!;
            var installationPath = InstallationPathTextBlock.Text;
            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() => { InstallButton.IsEnabled = false; });
                var versionArchive = GitHubController.DownloadRelease(selectedVersion, (progress) => OnUpdateUpdateProgress($"Downloading... {progress*100:0.00}%"));
                ClientController.UnpackZipUpdate(versionArchive, installationPath, (progress) => OnUpdateUpdateProgress($"Updating... {progress*100:0.00}%"));
                GitHubController.CloseStreams();
                this.Dispatcher.Invoke(() => { UpdateInstallButtonText(); InstallButton.IsEnabled = true; });
            });
        }

        private void OnUpdateUpdateProgress(FormattableString str)
        {
            this.Dispatcher.Invoke(() => { InstallButton.Content = Localizer.Do(str); });
        }
    }
}
