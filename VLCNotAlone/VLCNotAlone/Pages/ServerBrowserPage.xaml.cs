using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VLCNotAlone.Models;
using VLCNotAlone.Services.Networking;
using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;
using VLCNotAlone.Shared.Networking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone.Pages
{
    public enum ServerBrowserPageStartupType
    {
        Discover,
        Official,
        Favorite
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ServerBrowserPage : ContentPage, IDisposable
    {
        public ObservableCollection<ServerListItem> Servers { get; } = new ObservableCollection<ServerListItem>();

        private ServerBrowserPageStartupType _startupType;
        private CancellationTokenSource _discoverCts = new CancellationTokenSource();

        public ServerBrowserPage(ServerBrowserPageStartupType startupType)
        {
            InitializeComponent();
            this.BindingContext = this;

            _startupType = startupType;
            NetworkClient.Obj.OfficialServerDiscovered += OnServerDiscovered;
            NetworkClient.Obj.ServerDiscovered += OnServerDiscovered;
            OnRefresh();
        }

        private void OnRefresh()
        {
            Servers.Clear();

            switch (_startupType)
            {
                case ServerBrowserPageStartupType.Discover:
                    OnDiscover();
                    break;
                case ServerBrowserPageStartupType.Favorite:
                    OnFavorite();
                    break;
                case ServerBrowserPageStartupType.Official:
                    OnOfficial();
                    break;
            }
        }

        private void OnDiscover()
        {
            NetworkClient.Obj.StartDiscoverServers();
        }

        private void OnFavorite()
        {

        }

        private void OnOfficial()
        {

        }

        private void OnError()
        {

        }

        private void OnServerDiscovered(ListingHostInfo server)
        {
            var serverListItem = ServerListItem.FromListingHostInfo(server);
            Servers.Add(serverListItem);
        }

        private void OnServerSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var serverEntry = e.SelectedItem as ServerListItem;

            ServerNameLabel.Text = serverEntry?.Name;
            ServerDescriptionLabel.Text = serverEntry?.Description;
            ConnectButton.IsEnabled = serverEntry?.Version == Constants.AppVersion;
        }

        private void OnConnectButtonClicked(object sender, EventArgs e)
        {
            var serverEntry = ServersList.SelectedItem as ServerListItem;
            if (serverEntry == null)
                return;

            Navigation.PushModalAsync(new ConnectingProcessPage($"{serverEntry.Protocol}://{serverEntry.IpAddress}:{serverEntry.Port}/server"));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Dispose();
        }

        public void Dispose()
        {
            NetworkClient.Obj.OfficialServerDiscovered -= OnServerDiscovered;
            NetworkClient.Obj.ServerDiscovered -= OnServerDiscovered;
            _discoverCts.Cancel();
        }
    }
}