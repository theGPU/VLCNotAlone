using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;
using VLCNotAlone.Shared.Networking;

namespace VLCNotAlone.Networking
{
    internal sealed class NetworkClient : IDisposable
    {
        public static NetworkClient Obj { get; private set; } = new NetworkClient();

        public bool IsConnected => connection.State == HubConnectionState.Connected;
        HubConnection connection;
        public event Action OnConnected;
        public event Action<string, bool, Exception> OnDisconnected; //msg, isException, Exception?
        public event Action OnReconnecting;
        public event Action OnReconnected;

        public event Action<ListingHostInfo> ServerDiscovered;
        public event Action<ListingHostInfo> OfficialServerDiscovered;
        public event Action<Exception> MasterServerClientThrowDiscoverError;

        private readonly MasterServerClient masterServerClient;

        private NetworkClient()
        {
            masterServerClient = new MasterServerClient();
        }

        #region MasterServer
        public void StartDiscoverServers() => Task.Run(() => StartDiscoverServersInternal(false));
        public void StartDiscoverOfficialServers() => Task.Run(() => StartDiscoverServersInternal(true));

        private async Task StartDiscoverServersInternal(bool onlyOfficial)
        {
            try
            {
                var serverList = onlyOfficial ? await masterServerClient.TryRequestOfficialHostList() : await masterServerClient.TryRequestHostList();
                foreach (var server in serverList)
                {
                    if (server.IsOfficial)
                    {
                        OfficialServerDiscovered?.Invoke(server);
                    }
                    else
                    {
                        ServerDiscovered?.Invoke(server);
                    }
                }
            } catch (Exception ex)
            {
                MasterServerClientThrowDiscoverError?.Invoke(ex);
            }
        }
        #endregion MasterServer

        #region Client

        public async Task<Exception> TryConnectTo(string server)
        {
            connection = new HubConnectionBuilder().WithUrl(server).WithAutomaticReconnect(Enumerable.Range(0, 30).Select(x => TimeSpan.FromSeconds(2)).ToArray()).Build();
            connection.KeepAliveInterval = Constants.ClientToServerKeepAliveInterval;
            connection.Closed += OnHubConnectionClosed;
            connection.Reconnecting += OnHubConnectionReconnecting;
            connection.Reconnected += OnHubConnectionReconnected;
            RegisterDefaultListeners();

            try
            {
                await connection.StartAsync();
                OnConnected?.Invoke();
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

        private void RegisterDefaultListeners()
        {
            connection.On<string, string>("Receive", NetworkProcessor.Receive);
        }

        private async Task OnHubConnectionClosed(Exception ex)
        {
            OnDisconnected?.Invoke("Connection closed", ex != null, ex);
        }

        private async Task OnHubConnectionReconnecting(Exception ex)
        {
            OnReconnecting?.Invoke();
        }

        private async Task OnHubConnectionReconnected(string connectionId)
        {
            OnReconnected?.Invoke();
        }

        #endregion CLient

        #region ServerRPC

        public async Task Send(string username, string message)
        {
            await connection.InvokeAsync("Send", username, message);
        }

        #endregion ServerRPC

        public void Dispose()
        {
            masterServerClient.Dispose();
        }
    }
}
