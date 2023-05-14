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
            Task.Run(Connect);
        }

        private async Task Connect()
        {
            NetworkClient.Obj.OnConnected += OnConnected;
            NetworkClient.Obj.OnDisconnected += Obj_OnDisconnected;
            NetworkClient.Obj.OnReconnecting += Obj_OnReconnecting;
            NetworkClient.Obj.OnReconnected += Obj_OnReconnected;

            var exception = await NetworkClient.Obj.TryConnectTo(_serverUrl);
            if (exception != null)
            {
                OnError(exception.Message);
            }

            StatusList.Add("Requesting server info...");
        }

        private void OnConnected()
        {
            StatusList.Add("Connected");
        }

        private void Obj_OnDisconnected(string reason, bool isException, Exception exception)
        {
            if (isException)
                StatusList.Add(exception.Message);

            StatusList.Add($"Disconnected");
        }

        private void Obj_OnReconnecting()
        {
            StatusList.Add($"Reconnecting...");
        }

        private void Obj_OnReconnected()
        {
            StatusList.Add($"Reconnected");
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
            NetworkClient.Obj.OnDisconnected -= Obj_OnDisconnected;
            NetworkClient.Obj.OnReconnecting -= Obj_OnReconnecting;
            NetworkClient.Obj.OnReconnected -= Obj_OnReconnected;

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