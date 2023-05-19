using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VLCNotAlone.Networking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VLCNotAlone.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectingProcessPage : ContentPage
    {
        private string _serverUrl;
        private bool _keepConnectionOnDisappearing = false;
        public ObservableCollection<string> StatusList { get; } = new ObservableCollection<string>();

        public ConnectingProcessPage(string serverUrl)
        {
            InitializeComponent();
            BindingContext = this;
            _serverUrl = serverUrl;
        }

        protected override void OnAppearing()
        {
            StatusList.Add($"Connecting to {_serverUrl}...");
            Task.Run(Connect).ContinueWith(t => StatusList.Add($"Error in connect method: {t.Exception.InnerException.Message}"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task Connect()
        {
            NetworkClient.Obj.OnConnected += OnConnected;
            NetworkClient.Obj.OnDisconnected += OnDisconnected;
            NetworkClient.Obj.OnReconnecting += OnReconnecting;
            NetworkClient.Obj.OnReconnected += OnReconnected;
            NetworkClient.Obj.OnHubError += OnHubError;

            var exception = await NetworkClient.Obj.TryConnectTo(_serverUrl);
            if (exception != null)
            {
                OnError(exception.Message);
                return;
            }

            StatusList.Add("Requesting server info...");
            NetworkClient.Obj.ClearConnectionData();
            NetworkClient.Obj.ConnectionData.HostInfo = await NetworkClient.Obj.RequestHostInfo();
            StatusList.Add("Host info recieved");
            StatusList.Add($"Sync to {NetworkClient.Obj.ConnectionData.HostInfo.Name}...");
            if (NetworkClient.Obj.ConnectionData.HostInfo.HasPassword)
            {
                var passwordEnterPage = new PasswordEnterPage();
                passwordEnterPage.Disappearing += (object sender, EventArgs e) => _ = OnPasswordEnteringComplete(passwordEnterPage.Password);
                Device.BeginInvokeOnMainThread(async () => await Navigation.PushModalAsync(passwordEnterPage));
            } else
            {
                _ = OnPasswordEnteringComplete("");
            }
        }

        private async Task OnPasswordEnteringComplete(string password)
        {
            if (string.IsNullOrEmpty(password) && NetworkClient.Obj.ConnectionData.HostInfo.HasPassword)
            {
                StatusList.Add("Canceled by user");
                return;
            }

            await NetworkClient.Obj.CheckPassword(password);
            _ = Task.Run(GetRoomsList).ContinueWith(t => StatusList.Add($"Error in GetRoomsList method: {t.Exception.InnerException.Message}"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private async Task GetRoomsList()
        {
            StatusList.Add("Requesting rooms...");
            var rooms = await NetworkClient.Obj.RequestRoomsList();

            NetworkClient.Obj.ConnectionData.Rooms.Clear();
            rooms.ToList().ForEach(x => NetworkClient.Obj.ConnectionData.Rooms.AddOrUpdate(x.Id, x, (k, v) => x));

            _keepConnectionOnDisappearing = true;
            var serverOverviewPage = new ServerOverviewPage();
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushModalAsync(serverOverviewPage));
        }

        private void OnConnected()
        {
            StatusList.Add("Connected");
        }

        private void OnDisconnected(string reason, bool isException, Exception exception)
        {
            if (isException)
                StatusList.Add(exception.Message);

            StatusList.Add($"Disconnected");
        }

        private void OnReconnecting()
        {
            StatusList.Add($"Reconnecting...");
        }

        private void OnReconnected()
        {
            StatusList.Add($"Reconnected");
        }

        private void OnHubError(Exception obj)
        {
            StatusList.Add($"Hub error: {obj.Message}");
        }

        private void OnError(string error)
        {
            StatusList.Add(error);
            StatusList.Add($"Connecting failed");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            NetworkClient.Obj.OnConnected -= OnConnected;
            NetworkClient.Obj.OnDisconnected -= OnDisconnected;
            NetworkClient.Obj.OnReconnecting -= OnReconnecting;
            NetworkClient.Obj.OnReconnected -= OnReconnected;
            NetworkClient.Obj.OnHubError -= OnHubError;

            if (!_keepConnectionOnDisappearing)
            {
                NetworkClient.Obj.DisposeConnection();
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}