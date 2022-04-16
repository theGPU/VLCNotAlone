using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAloneShared.Enums;
using WatsonTcp;

namespace VLCNotAloneShared
{
    public class ClientApiV3
    {
        public string ClientName { get; set; }
        public string Username;

        public string host;
        public int port;

        public Action<bool> OnInRoomChanged;
        private bool _inRoom = false;
        public bool InRoom { get { return _inRoom; } set { _inRoom = value; OnInRoomChanged?.Invoke(value); } }

        public ServerTypes ConnectedServerType;
        public Action OnHelloAccepted;

        private WatsonTcpClient Client;
        private int ClientId;

        public Action<bool, string, string> OnEvent; //isError, title, desc
        public Action OnDisconnect;

        public Action<string> OnSetLocalMediaFile;
        public Action<string> OnSetGlobalMediaFile;
        public Action<string> OnSetInternetMediaFile;
        public Action<long> OnSetTimeRecived;
        public Action OnPause;
        public Action OnResume;

        public Action<string> OnClientConnected;
        public Action<string> OnClientDisconnected;

        public Action<int> OnRequestRoomContent;
        public Action<string, string, long> OnWhatTimeResponce;

        public Action<string[]> OnClientsList;

        public void Connect()
        {
            ClientId = new Random().Next();
            Client?.Dispose();

            Client = new WatsonTcpClient(host, port);

            Client.Events.ServerConnected += (s, e) => OnServerConnected(e);
            Client.Events.MessageReceived += (s, e) => OnMessageRecived(Encoding.UTF8.GetString(e.Data), e.Metadata);
            Client.Events.ServerDisconnected += (s, e) => OnServerDisconnected(e);
            Client.Events.ExceptionEncountered += (s, e) => OnExceptionEncountered(e);

            OnInRoomChanged += (v) => { if (v) RequestRoomContent(); };

            OnEvent?.Invoke(false, "Connect", "Connecting...");
            Client.Connect();
        }

        public void TryConnectToDefaultRoom()
        {
            if (ConnectedServerType == ServerTypes.SingleRoom)
                return;

            OnEvent?.Invoke(true, "Room", $"Trying to connect to Default room...");
            Client.Send("ConnectToRoom", new Dictionary<object, object>() {
                { ConnectToRoomMessageMetadataTypes.ClientId, this.ClientId },
                { ConnectToRoomMessageMetadataTypes.Username, this.Username ?? Environment.UserName },
                { ConnectToRoomMessageMetadataTypes.RoomName, "Default" },
                { ConnectToRoomMessageMetadataTypes.Password, "" }
            });
        }

        private void OnServerDisconnected(DisconnectionEventArgs e)
        {
            OnEvent?.Invoke(true, "Connect", $"Disconnected: {Enum.GetName(e.Reason)}");
            InRoom = false;
        }

        private void OnExceptionEncountered(ExceptionEventArgs e)
        {
            OnEvent?.Invoke(true, "Connect", $"Exception: {e.Exception}");
            InRoom = false;
        }

        private void OnServerConnected(ConnectionEventArgs e)
        {
            OnEvent?.Invoke(false, "Connect", "Connected");
            OnEvent?.Invoke(false, "Connect", "Sending hello");
            SendHelloMessage();
        }

        private void SendHelloMessage()
        {
            Client.Send("HelloMessageFromClient", new Dictionary<object, object>()
            {
                { ClientHelloMessageMetadataTypes.ApiVersion, 3 },
                { ClientHelloMessageMetadataTypes.ClientName, ClientName }
            });
        }

        private void OnMessageRecived(string message, Dictionary<object, object> metadata)
        {
            switch (message)
            {
                case "HelloMessageFromServer":
                    OnHelloMessageFromServer(metadata);
                    break;
                case "SuccessConnectToRoom":
                    OnConnectToRoomResponse(true);
                    break;
                case "FailConnectToRoom":
                    OnConnectToRoomResponse(false);
                    break;
                case "SetRoomStage":
                    OnSetRoomStage(metadata);
                    break;
                case "ClientConnectedNotice":
                    OnEvent?.Invoke(false, "Room", $"Client connected: {metadata[Enum.GetName(ClientConnectedNoticeMetadataTypes.Username)].ToString()}");
                    break;
                case "ClientDisconnectedNotice":
                    OnEvent?.Invoke(false, "Room", $"Client disconnected: {metadata[Enum.GetName(ClientDisconnectedNoticeMetadataTypes.Username)].ToString()}");
                    break;
                case "ContentClientRequest":
                    SendRoomContent(metadata);
                    break;
                case "ContentSync":
                    OnContentSync(metadata);
                    break;
                default:
                    break;
            }
        }

        private void OnHelloMessageFromServer(Dictionary<object, object> metadata)
        {
            OnEvent?.Invoke(false, "Connect", "Successful");
            ConnectedServerType = (ServerTypes) (long) metadata[Enum.GetName(ServerHelloMessageMetadataTypes.ServerType)];
            OnHelloAccepted?.Invoke();
        }

        private void OnConnectToRoomResponse(bool successful)
        {
            InRoom = successful;
            OnEvent?.Invoke(false, "Room", $"Connection {(successful ? "successful" : "failed")}");
        }

        private void OnSetRoomStage(Dictionary<object, object> newStage)
        {
            if (newStage.TryGetValue(Enum.GetName(SetRoomStageMetadataTypes.Filename), out var filenameEntry) && newStage.TryGetValue(Enum.GetName(SetRoomStageMetadataTypes.Mode), out var modeEntry))
            {
                var filename = filenameEntry.ToString();
                var mode = (RoomFileMode)(long)modeEntry;
                switch (mode)
                {
                    case RoomFileMode.Local: { OnSetLocalMediaFile(filename); break; }
                    case RoomFileMode.Global: { OnSetGlobalMediaFile(filename); break; }
                    case RoomFileMode.Internet: { OnSetInternetMediaFile(filename); break; }
                }
            }

            if (newStage.TryGetValue(Enum.GetName(SetRoomStageMetadataTypes.Paused), out var pausedEntry))
            {
                var pause = (bool)pausedEntry;
                if (pause)
                    OnPause?.Invoke();
                else
                    OnResume?.Invoke();
            }

            if (newStage.TryGetValue(Enum.GetName(SetRoomStageMetadataTypes.Position), out var positionEntry))
            {
                var position = (long)positionEntry;
                OnSetTimeRecived?.Invoke(position);
            }
        }

        private void RequestRoomContent() => Client.Send("ContentServerRequest");

        private void SendRoomContent(Dictionary<object, object> metadata) => OnRequestRoomContent?.Invoke((int)(long)metadata[Enum.GetName(ContentClientRequestMetadataTypes.RequesterId)]);

        private void OnContentSync(Dictionary<object, object> newStage)
        {
            if (newStage.TryGetValue(Enum.GetName(ContentSyncMetadataTypes.Filename), out var filenameEntry) && newStage.TryGetValue(Enum.GetName(ContentSyncMetadataTypes.Mode), out var modeEntry))
            {
                var filename = filenameEntry.ToString();
                var mode = (RoomFileMode)(long)modeEntry;
                switch (mode)
                {
                    case RoomFileMode.Local: { OnSetLocalMediaFile(filename); break; }
                    case RoomFileMode.Global: { OnSetGlobalMediaFile(filename); break; }
                    case RoomFileMode.Internet: { OnSetInternetMediaFile(filename); break; }
                }
            }

            if (newStage.TryGetValue(Enum.GetName(ContentSyncMetadataTypes.Position), out var positionEntry))
            {
                var position = (long)positionEntry;
                OnSetTimeRecived?.Invoke(position);
            }
        }

        public void SetMediaFile(string path, RoomFileMode fileMode) => Client.Send("SetRoomStage", new Dictionary<object, object>() {
            { SetRoomStageMetadataTypes.Mode, fileMode },
            { SetRoomStageMetadataTypes.Filename, path },
            { SetRoomStageMetadataTypes.Paused, true },
            { SetRoomStageMetadataTypes.Position, 0L }
        });

        public void SetTime(long time) => Client.Send("SetRoomStage", new Dictionary<object, object>() { { SetRoomStageMetadataTypes.Position, time } });

#warning link
        public void RequestClientList() { }

        public void SetPause(bool pause, long? time = null)
        {
            var metadata = new Dictionary<object, object>();
            metadata.Add(SetRoomStageMetadataTypes.Paused, pause);
            if (time.HasValue)
                metadata.Add(SetRoomStageMetadataTypes.Position, time);
            Client.Send("SetRoomStage", metadata);
        }

        public void SendRoomContentResponse(int clientId, string currentFileMode, string currentFileName, long time) => Client.Send("ContentClientResponse", new Dictionary<object, object>() {
            { ContentClientResponseMetadataTypes.ReciverId, clientId },
            { ContentClientResponseMetadataTypes.Mode, currentFileMode == "Local" ? RoomFileMode.Local : currentFileMode == "Global" ? RoomFileMode.Global : RoomFileMode.Internet },
            { ContentClientResponseMetadataTypes.Filename, currentFileName },
            { ContentClientResponseMetadataTypes.Position, time }
        });
    }
}
