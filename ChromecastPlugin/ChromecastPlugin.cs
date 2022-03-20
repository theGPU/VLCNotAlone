using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VLCNotAlone;
using VLCNotAlone.Plugins.Interfaces;

namespace ChromecastPlugin
{
    public class ChromecastPlugin : IInitializablePlugin
    {
        public string Name => "ChromecastPlugin";
        public string Description => "Sample plugin.\nAdd chromecast menu.";

        private RendererDiscoverer rendererDiscoverer;
        private MenuItem pluginMenu;

        private List<MenuItem> renderersOptions = new List<MenuItem>();

        public void Initialize()
        {
            var localMenuItem = new MenuItem() { IsChecked = true, Header = "Local", Tag = null };
            localMenuItem.Click += OnSelectRenderer;
            renderersOptions.Add(localMenuItem);

            MainWindow.Instance.OnMediaPlayerLoaded += (p) => PostInit();
        }

        private void PostInit()
        {
            pluginMenu = new MenuItem() { Header = "Chromecast" };
            pluginMenu.ItemsSource = renderersOptions;
            MainWindow.ControlMenu.Items.Add(pluginMenu);

            rendererDiscoverer = new RendererDiscoverer(MainWindow.Instance.libVLC);

            rendererDiscoverer.ItemAdded += (s, e) => {
                MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    var menuItem = new MenuItem() { Header = e.RendererItem.Name, Tag = e.RendererItem };
                    menuItem.Click += OnSelectRenderer;
                    renderersOptions.Add(menuItem);
                    pluginMenu.Items.Refresh();
                }); 
            };
            rendererDiscoverer.ItemDeleted += (s, e) => { 
                MainWindow.Instance.Dispatcher.Invoke(() => {
                    var disconnectedRenderer = renderersOptions.Single(x => (x.Tag as RendererItem)?.Name == e.RendererItem.Name);
                    renderersOptions.Remove(disconnectedRenderer);
                    pluginMenu.Items.Refresh();

                    if (disconnectedRenderer.IsChecked)
                        OnSelectRenderer(renderersOptions.Single(x => x.Tag == null), null);
                });
            };

            rendererDiscoverer.Start();
        }

        private void OnSelectRenderer(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem.IsChecked)
                return;

            var checkedElement = renderersOptions.SingleOrDefault(x => x.IsChecked);
            if (checkedElement != null)
                checkedElement.IsChecked = false;

            menuItem.IsChecked = true;

            MainWindow.Instance.mediaPlayer.SetRenderer((RendererItem) menuItem.Tag);
        }
    }
}
