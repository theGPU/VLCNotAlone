using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading;
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
        public event Action<Exception> OnHubError;

        public event Action<ListingHostInfo> ServerDiscovered;
        public event Action<ListingHostInfo> OfficialServerDiscovered;
        public event Action<Exception> MasterServerClientThrowDiscoverError;

        private readonly MasterServerClient masterServerClient;

        private CancellationToken _cancellationToken;

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

        public async Task<Exception> TryConnectTo(string server, CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            try
            {
                if (connection != null)
                    await connection.DisposeAsync();

                connection = new HubConnectionBuilder().WithUrl(server).WithAutomaticReconnect(Enumerable.Range(0, 30).Select(x => TimeSpan.FromSeconds(2)).ToArray()).Build();
                connection.KeepAliveInterval = Constants.ClientToServerKeepAliveInterval;
                connection.Closed += OnHubConnectionClosed;
                connection.Reconnecting += OnHubConnectionReconnecting;
                connection.Reconnected += OnHubConnectionReconnected;
                RegisterDefaultListeners();
            
                await connection.StartAsync(_cancellationToken);
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

        #endregion Client

        #region ClientRPC

        #endregion

        #region ServerRPC

        public async Task<BaseHostInfo> RequestHostInfo()
        {
            return await RPC<BaseHostInfo>("GetHostInfo", new object[] { }, _cancellationToken);
        }

        private async Task<T> RPC<T>(string methodName, object[] args, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                return await connection.InvokeCoreAsync<T>(methodName, args, _cancellationToken);
            } catch (Exception ex)
            {
                OnHubError?.Invoke(ex);
            }

            return null;
        }

        #endregion ServerRPC

        public void DisposeConnection()
        {
            connection.DisposeAsync();
        }

        public void Dispose()
        {
            masterServerClient.Dispose();
            connection.DisposeAsync();
        }
    }
}
