using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonTcp;

namespace VLCNotAloneMultiRoomServer.Controllers
{
    internal static class ServerListenerController
    {
        private static WatsonTcpServer server;

        public static Action<string> OnClientConnected;
        public static Action<string, DisconnectReason> OnClientDisconnected;
        public static Action<string, string, Dictionary<object, object>> OnMessageRecived;
        public static Action<Exception, string> OnExceptionEncountered;
        public static Action OnServerStarted;

        public static IEnumerable<string> ListClients => server.ListClients();

        internal static void StartServer(string address, int port)
        {
            server = new WatsonTcpServer(address, port);
            server.Events.ClientConnected += (s, e) => OnClientConnected?.Invoke(e.IpPort);
            server.Events.ClientDisconnected += (s, e) => OnClientDisconnected?.Invoke(e.IpPort, e.Reason);
            server.Events.MessageReceived += (s, e) => OnMessageRecived?.Invoke(e.IpPort, Encoding.UTF8.GetString(e.Data), e.Metadata);
            server.Events.ExceptionEncountered += (s, e) => OnExceptionEncountered?.Invoke(e.Exception, e.Json);
            server.Events.ServerStarted += (s, e) => OnServerStarted?.Invoke();

            server.Start();
        }

        internal static void DisconnectClient(string clientAddress, MessageStatus status = MessageStatus.Normal, string reason = null) => server.DisconnectClient(clientAddress, MessageStatus.Normal, true);
        internal static void SendMessage(string clientAddress, string message, Dictionary<object, object> metadata = null) => server.Send(clientAddress, message, metadata);
        internal static async Task SendMessageAsync(string clientAddress, string message, Dictionary<object, object> metadata = null) => await server.SendAsync(clientAddress, message, metadata);

        internal static void Shutdown()
        {
            server.DisconnectClients();
            server.Stop();
            server.Dispose();
        }
    }
}
