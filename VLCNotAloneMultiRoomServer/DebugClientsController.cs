using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonTcp;

namespace VLCNotAloneMultiRoomServer
{
    internal class DebugClient : IDisposable
    {
        readonly WatsonTcpClient client;

        public static Action<DebugClient> OnServerConnected;
        public static Action<DebugClient, DisconnectReason> OnServerDisconnected;
        public static Action<DebugClient, string, Dictionary<object, object>> OnMessageRecived;
        public static Action<DebugClient, Exception, string> OnExceptionEncountered;

        public DebugClient(string ip, int port)
        {
            client = new WatsonTcpClient(ip, port);
            client.Events.ServerConnected += (s, e) => OnServerConnected?.Invoke(this);
            client.Events.ServerDisconnected += (s, e) => OnServerDisconnected?.Invoke(this, e.Reason);
            client.Events.MessageReceived += (s, e) => OnMessageRecived?.Invoke(this, Encoding.UTF8.GetString(e.Data), e.Metadata);
            client.Events.ExceptionEncountered += (s, e) => OnExceptionEncountered?.Invoke(this, e.Exception, e.Json);
        }

        public void Connect() => client.Connect();

        public void Disconnect() => client.Disconnect();
        public void SendMessage(string data, Dictionary<object, object> metadata = null) => client.Send(data, metadata);
        public void Dispose() => client.Dispose();
    }
}
