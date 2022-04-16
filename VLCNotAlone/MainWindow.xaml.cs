using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VLCNotAloneShared;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using VLCNotAlone.Controllers;
using VLCNotAlone.Plugins.Controllers;

using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;
using VLCNotAlone.Utils;
using VLCNotAloneShared.Enums;

namespace VLCNotAlone
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public static MenuItem PluginsClientMenu { get; private set; }
        public static Menu ControlMenu { get; private set; }

        public LibVLC libVLC;
        public MediaPlayer mediaPlayer;
        public long currentLength { get; private set; } = 0;

        private int currentCropGeometry = 0;
        private static string[] cropGeometries = { "", "16:10", "16:9", "4:3", "13:7", "11:5", "7:3", "5:3", "5:4", "1:1" };

        public readonly ClientApiV3 clientApi = new ClientApiV3();

        public Action<MediaPlayer> OnMediaPlayerLoaded;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            PluginsClientMenu = this.ClientMenu;
            ControlMenu = this.TopControlMenu;

            CurrentTimeLabel.Content = "00:00:00";
            MaxTimeLabel.Content = "00:00:00";
            VideoProgressBar.Value = 0;

            PluginsManager.Init();

            VideoPlayer.Loaded += (s, e) =>
            {
                Core.Initialize();

#if DEBUG
                libVLC = new LibVLC("--verbose=2");
                //libVLC.SetLogFile($"./VlcNotAloneLogs {DateTime.Now}.txt");
#else
                libVLC = new LibVLC();
#endif

                mediaPlayer = new MediaPlayer(libVLC);
                mediaPlayer.TimeChanged += (s, e) => OnPlayerTimeChanged(e.Time);

                mediaPlayer.LengthChanged += (s, e) =>
                {
                    currentLength = e.Length;
                    var t = TimeSpan.FromMilliseconds(e.Length);
                    this.Dispatcher.Invoke(() => MaxTimeLabel.Content = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds));
                    this.Dispatcher.Invoke(() => RefilChannelsMenus());
                };

                mediaPlayer.Playing += (s, e) => this.Dispatcher.Invoke(() => CheckSelectedTracks()); //при запуске и снятия с паузы

                VideoPlayer.MediaPlayer = mediaPlayer;
                OnMediaPlayerLoaded?.Invoke(mediaPlayer);

                ConfigController.Init();
                Localizer.Init();

                LogoImagePlayer.Visibility = ConfigController.ShowLogo ? Visibility.Visible : Visibility.Collapsed;
                LogoImagePlayer.OnPlayingEnd += () => { LogoImagePlayer.Visibility = Visibility.Collapsed; ((Grid)LogoImagePlayer.Parent).Children.Remove(LogoImagePlayer); };
            };

            clientApi.OnInRoomChanged += (connected) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MediaMenu.IsEnabled = connected;
                    AudioMenu.IsEnabled = connected;
                    VideoMenu.IsEnabled = connected;
                    SubtitlesMenu.IsEnabled = connected;
                    ServerMenu.IsEnabled = connected;
                });
            };
            clientApi.OnDisconnect += () =>
            {
                mediaPlayer!.SetPause(true);
                OnShowMessage(Localizer.DoStr("Connection"), Localizer.DoStr("aborted"));
                clientApi.Connect();
                //if (clientApi.Connected)
                //    clientApi.Pause(mediaPlayer.Time);
            };
            clientApi.OnSetTimeRecived += (time) =>
            {
                mediaPlayer!.Time = time;
                OnPlayerTimeChanged(time);
            };
            clientApi.OnPause += () => mediaPlayer!.SetPause(true);
            clientApi.OnResume += () => mediaPlayer!.SetPause(false);

            clientApi.OnSetLocalMediaFile += (mediaPath) => PlayNewFile(mediaPath, "Local");
            clientApi.OnSetGlobalMediaFile += (mediaPath) => PlayNewFile(mediaPath, "Global");
            clientApi.OnSetInternetMediaFile += (mediaPath) => PlayNewFile(mediaPath, "Internet");

            clientApi.OnClientConnected += (endpoint) =>
            {
                mediaPlayer!.SetPause(true);
                OnShowMessage(Localizer.DoStr("Client connected"), endpoint);
            };
            clientApi.OnClientDisconnected += (endpoint) =>
            {
                mediaPlayer!.SetPause(true);
                OnShowMessage(Localizer.DoStr("Client disconnected"), endpoint);
            };

            clientApi.OnRequestRoomContent += (clientId) => 
            {
                if (mediaPlayer!.Media != null)
                    clientApi.SendRoomContentResponse(clientId, CurrentFileMode, CurrentFileName, mediaPlayer!.Time);
            };
            clientApi.OnWhatTimeResponce += (mode, path, time) =>
            {
                PlayNewFile(path, mode);
                mediaPlayer!.Time = time;
            };

            clientApi.OnClientsList += (clients) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    for (var i = ClientsListMenu.Items.Count - 1; i >= 0; i--)
                    {
                        var item = (MenuItem)ClientsListMenu.Items[i];
                        if (item.Name != "UpdateClientsListMenuItem")
                            ClientsListMenu.Items.Remove(item);
                    }

                    foreach (var client in clients)
                        ClientsListMenu.Items.Add(new MenuItem() { Header = client });

                    ClientsListMenu.Items.Refresh();
                    ClientsListMenu.UpdateLayout();
                });
            };

            clientApi.OnEvent += (isError, title, desc) => OnShowMessage(title, desc);

            clientApi.OnHelloAccepted += () => { if (ConfigController.ConnectToDefaultRoomAutomatically) clientApi.TryConnectToDefaultRoom(); };

            FillServersList();

            //config controller

            ConfigController.OnFileCachingTimeChanged += (newCachingTime) =>
            {
                mediaPlayer!.FileCaching = newCachingTime;
                FileCacheTextBox.Text = newCachingTime.ToString();
            };

            ConfigController.OnNetworkCachingTimeChanged += (newCachingTime) =>
            {
                mediaPlayer!.NetworkCaching = newCachingTime;
                NetworkCacheTextBox.Text = newCachingTime.ToString();
            };

            ConfigController.OnConfigControllerInited += () =>
            {
                NicknameTextBox.Text = ConfigController.Nickname;
            };
        }

        private void FillServersList()
        {
            var items = File.ReadAllLines("Servers.txt").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => { var menu = new MenuItem() { Header = x }; menu.Click += OnConnectAddressClick; return menu; });
            this.Dispatcher.Invoke(() =>
            {
                foreach (var item in items)
                    ConnectionMenu.Items.Insert(0, item);

                ConnectionMenu.Items.Refresh();
                ConnectionMenu.UpdateLayout();
            });

        }

        private void OnPlayerTimeChanged(long time)
        {
            if (time < 0)
                time = 0;

            var t = TimeSpan.FromMilliseconds(time);
            this.Dispatcher.Invoke(() => {
                CurrentTimeLabel.Content = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
                if (currentLength > 0)
                    VideoProgressBar.Value = (double)time / currentLength;
            });
        }

        private static string FindGlobalFile(string fileName)
        {
            EverythingController.Everything_SetSearchW(fileName);
            EverythingController.Everything_SetRequestFlags(EverythingController.EVERYTHING_REQUEST_PATH | EverythingController.EVERYTHING_REQUEST_FILE_NAME);
            EverythingController.Everything_QueryW(true);
            if (EverythingController.Everything_GetNumFileResults() > 0)
            {
                var path = Path.Combine(Marshal.PtrToStringAuto(EverythingController.Everything_GetResultPath(0))!, fileName);
                return path;
            }

            var pathes = Environment.GetEnvironmentVariable("Path")!.Split(';');
            foreach (var pathElement in pathes)
            {
                var path = $@"{pathElement}\{fileName}";
                if (File.Exists(path))
                    return path;
            }

            return fileName;
        }

        public string? CurrentFileMode { get; private set; }
        public string? CurrentFileName { get; private set; }
        public string? CurrentFileFullPatch { get; private set; }
        public string? CurrentVideoTitle { get; private set; }
        public Action<bool> OnPlayNewFileResult;

        private bool PlayNewFile(string path, string mode)
        {
            CurrentFileName = path;
            CurrentFileMode = mode;
            var filePath = mode == "Global" ? FindGlobalFile(path) : path;
            CurrentFileFullPatch = filePath;

            var media = new Media(libVLC, filePath, mode == "Internet" ? FromType.FromLocation : FromType.FromPath);
            if (mode == "Internet")
            {
                OnShowMessage(Localizer.DoStr("Internet parser"), Localizer.DoStr("loading..."));
                media.Parse(MediaParseOptions.ParseNetwork).Wait();
                OnShowMessage(Localizer.DoStr("Internet parser"), Localizer.DoStr("loaded"));
            }

            if (mediaPlayer!.Play(media.SubItems.Count > 0 ? media.SubItems.First() : media))
            {
                mediaPlayer.SetPause(true);
                OnPlayerTimeChanged(0);
                using var file = TagLib.File.Create(CurrentFileFullPatch);
                CurrentVideoTitle = file.Tag.Title;
                this.Dispatcher.Invoke(() => this.Title = $"{CurrentVideoTitle ?? CurrentFileName} - VLCNotAlone");
                OnPlayNewFileResult?.Invoke(true);
                return true;
            }
            else
            {
                OnShowMessage(Localizer.DoStr("Load file"), Localizer.DoStr("error"));
                this.Dispatcher.Invoke(() => RefilChannelsMenus());

                CurrentFileName = null;
                CurrentFileMode = null;
                CurrentFileFullPatch = null;
                CurrentVideoTitle = null;

                OnPlayNewFileResult?.Invoke(false);
                return false;
            }
        }

        private void OnOpenLocalFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                clientApi.SetMediaFile(Path.GetFileName(openFileDialog.FileName), RoomFileMode.Local);
        }

        private void OnOpenGlobalFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                clientApi.SetMediaFile(Path.GetFileName(openFileDialog.FileName), RoomFileMode.Global);
        }

        private void OnURLEnterTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                clientApi.SetMediaFile(((TextBox)sender).Text, RoomFileMode.Internet);
            }
        }

        private void OnServerIPBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    var hostAndPort = ((TextBox)sender).Text.Split(':');
                    clientApi.host = hostAndPort[0];
                    clientApi.port = int.Parse(hostAndPort[1]);
                    clientApi.Connect();
                } catch (Exception ex)
                {
                    OnShowMessage(Localizer.DoStr("Connection"), Localizer.DoStr("Enter valid host. e.g. \"example.com:4096\""));
                }
            }
        }

        private void OnConnectAddressClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var hostPort = ((string)menuItem.Header).Split(':');
            clientApi.host = hostPort[0];
            clientApi.port = int.Parse(hostPort[1]);
            clientApi.Connect();
        }

        private void OnProgressBarDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var progressValue = e.GetPosition(VideoProgressBar).X / VideoProgressBar.ActualWidth * VideoProgressBar.Maximum;
            //mediaPlayer.Time = (long)(currentLength * progressValue);
            clientApi.SetTime((long)(currentLength * progressValue));
        }

        private void OnClickUpdateClientsList(object sender, RoutedEventArgs e) => clientApi.RequestClientList();

        private void OnNicknameBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var senderBox = (TextBox)sender;
                ConfigController.Nickname = senderBox.Text;
            }
        }

        private void OnFileCacheBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var senderBox = (TextBox) sender;
                if (uint.TryParse(senderBox.Text, out var newCacheTime))
                    ConfigController.SetFileCachingTime(newCacheTime);
                else
                    senderBox.Text = mediaPlayer.FileCaching.ToString();
            }
        }

        private void OnNetCacheBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var senderBox = (TextBox)sender;
                if (uint.TryParse(senderBox.Text, out var newCacheTime))
                    ConfigController.SetNetworkCachingTime(newCacheTime);
                else
                    senderBox.Text = mediaPlayer.NetworkCaching.ToString();
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                case Key.MediaPlayPause:
                    if (mediaPlayer.IsPlaying)
                        clientApi.SetPause(true, mediaPlayer.Time);
                    else 
                        clientApi.SetPause(false);
                    //mediaPlayer.Pause();
                    break;
                case Key.MediaStop:
                    if (mediaPlayer.IsPlaying)
                        clientApi.SetPause(true, mediaPlayer.Time);
                    break;
                case Key.C:
                    currentCropGeometry++;
                    if (currentCropGeometry == cropGeometries.Length)
                        currentCropGeometry = 0;

                    mediaPlayer.CropGeometry = cropGeometries[currentCropGeometry];
                    OnShowMessage(Localizer.DoStr("Crop"), cropGeometries[currentCropGeometry] != "" ? cropGeometries[currentCropGeometry] : Localizer.DoStr("default"));
                    break;
                case Key.Left:
                    clientApi.SetTime(mediaPlayer.Time - 5000 >= 0 ? mediaPlayer.Time - 5000 : 0);
                    //mediaPlayer.Time -= 5000;
                    break;
                case Key.Right:
                    clientApi.SetTime(mediaPlayer.Time + 5000 <= mediaPlayer.Length ? mediaPlayer.Time + 5000 : mediaPlayer.Length);
                    //mediaPlayer.Time += 5000;
                    break;
                case Key.MediaPreviousTrack:
                    clientApi.SetTime(0);
                    break;
                case Key.Up:
                case Key.VolumeUp:
                    mediaPlayer.Volume += 5;
                    break;
                case Key.Down:
                case Key.VolumeDown:
                    mediaPlayer.Volume = mediaPlayer.Volume - 5 >= 0 ? mediaPlayer.Volume-5 : 0;
                    break;
                case Key.VolumeMute:
                    mediaPlayer.Mute = mediaPlayer.Mute;
                    break;
                case Key.Q:
                case Key.Apps:
                    BottomControlPanel.Opacity = BottomControlPanel.Opacity == 0 ? 1 : 0;
                    TopControlMenu.Visibility = TopControlMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                case Key.F:
                case Key.Zoom:
                    if (this.WindowStyle != WindowStyle.SingleBorderWindow)
                    {
                        this.ResizeMode = ResizeMode.CanResize;
                        this.WindowStyle = WindowStyle.SingleBorderWindow;
                        this.WindowState = WindowState.Normal;
                        //this.Topmost = false;
                    }
                    else
                    {
                        this.ResizeMode = ResizeMode.NoResize;
                        this.WindowStyle = WindowStyle.None;
                        this.WindowState = WindowState.Maximized;
                        //this.Topmost = true;
                    }
                    break;
                case Key.SelectMedia:
                    OnOpenGlobalFile(null, null);
                    break;
            }
        }

        public void OnShowMessage(string prefix, string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                var messageLabel = new TextBlock();
                messageLabel.TextWrapping = TextWrapping.Wrap;
                messageLabel.Foreground = Brushes.White;
                messageLabel.FontSize = 16;
                messageLabel.Text = $"{prefix}: {text}";
                MessagesStackPanel.Children.Add(messageLabel);

                var messageHideTimer = new Timer() { AutoReset = false, Interval = 5000, Enabled = true};
                messageHideTimer.Elapsed += (s, e) => { this.Dispatcher.Invoke(() => MessagesStackPanel.Children.RemoveAt(0)); };
            });
        }

        private void RefilChannelsMenus()
        {
            AudioMenu.Items.Clear();
            VideoMenu.Items.Clear();
            SubtitlesMenu.Items.Clear();

            foreach (var audio in mediaPlayer.AudioTrackDescription)
            {
                Trace.WriteLine($"{audio.Name}");
                var newItem = new MenuItem() { Header = $"{audio.Name}", Name = "Audio", Tag = audio.Id };
                newItem.Click += OnClickChangeTrack;
                AudioMenu.Items.Add(newItem);
            }

            var loadAudioFromFileItem = new MenuItem() { Header = $"Load from file", Name = "AudioFromFile" };
            loadAudioFromFileItem.Click += OnClickChangeTrack;
            AudioMenu.Items.Add(loadAudioFromFileItem);

            foreach (var video in mediaPlayer.VideoTrackDescription)
            {
                Trace.WriteLine($"{video.Name}");
                var newItem = new MenuItem() { Header = $"{video.Name}", Name = "Video", Tag = video.Id };
                newItem.Click += OnClickChangeTrack;
                VideoMenu.Items.Add(newItem);
            }

            foreach (var subtitles in mediaPlayer.SpuDescription)
            {
                Trace.WriteLine($"{subtitles.Name}");
                var newItem = new MenuItem() { Header = $"{subtitles.Name}", Name = "Subtitles", Tag = subtitles.Id };
                newItem.Click += OnClickChangeTrack;
                SubtitlesMenu.Items.Add(newItem);
            }

            var loadSubFromFileItem = new MenuItem() { Header = $"Load from file", Name = "SubtitlesFromFile" };
            loadSubFromFileItem.Click += OnClickChangeTrack;
            SubtitlesMenu.Items.Add(loadSubFromFileItem);

            AudioMenu.Items.Refresh();
            AudioMenu.UpdateLayout();

            VideoMenu.Items.Refresh();
            VideoMenu.UpdateLayout();

            SubtitlesMenu.Items.Refresh();
            SubtitlesMenu.UpdateLayout();

            CheckSelectedTracks();
        }

        private void OnClickChangeTrack(object sender, RoutedEventArgs e)
        {
            var menuElement = (MenuItem) sender;
            if (menuElement.Name == "Audio")
            {
                mediaPlayer.SetAudioTrack((int)menuElement.Tag);
                menuElement.IsChecked = true;
            }
            else if (menuElement.Name == "AudioFromFile")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    if (mediaPlayer.AddSlave(MediaSlaveType.Audio, "file:///"+openFileDialog.FileName, true))
                    {
                        this.Dispatcher.Invoke(() => RefilChannelsMenus());
                        OnShowMessage(Localizer.DoStr("Audio"), Localizer.DoStr("file loaded"));
                    }
                    else
                        OnShowMessage(Localizer.DoStr("Audio"), Localizer.DoStr("error"));
                }
            }
            else if (menuElement.Name == "Video")
            {
                mediaPlayer.SetVideoTrack((int)menuElement.Tag);
            }
            else if (menuElement.Name == "Subtitles")
            {
                mediaPlayer.SetSpu((int)menuElement.Tag);
            }
            else if (menuElement.Name == "SubtitlesFromFile")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    if (mediaPlayer.AddSlave(MediaSlaveType.Subtitle, "file:///"+openFileDialog.FileName, true))
                    {
                        this.Dispatcher.Invoke(() => RefilChannelsMenus());
                        OnShowMessage(Localizer.DoStr("Subtitles"), Localizer.DoStr("file loaded"));
                    }
                    else
                        OnShowMessage(Localizer.DoStr("Subtitles"), Localizer.DoStr("error"));
                }
            }

            CheckSelectedTracks();
        }

        private void CheckSelectedTracks()
        {
            var audioTrackNum = mediaPlayer.AudioTrack;
            foreach (var item in AudioMenu.Items.Cast<MenuItem>().Where(x => x.Tag is int))
                item.IsChecked = audioTrackNum == (int)item.Tag;

            var videoTrackNum = mediaPlayer.VideoTrack;
            foreach (var item in VideoMenu.Items.Cast<MenuItem>().Where(x => x.Tag is int))
                item.IsChecked = videoTrackNum == (int)item.Tag;

            var spuTrackNum = mediaPlayer.Spu;
            foreach (var item in SubtitlesMenu.Items.Cast<MenuItem>().Where(x => x.Tag is int))
                item.IsChecked = spuTrackNum == (int)item.Tag;
        }
    }
}
