using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAloneMultiRoomServer.POCO;
using VLCNotAloneMultiRoomServer.Utils;
using VLCNotAloneShared.Enums;
using WatsonTcp;

namespace VLCNotAloneMultiRoomServer.Controllers
{
    internal static class ServerController
    {
        const string ApiVersion = "3";

        public static void Init()
        {
            ServerListenerController.OnServerStarted += () => Logger.WriteLine("ServerController", "Server started");

            ServerListenerController.OnMessageRecived += (sender, message, metadata) => Logger.WriteLine("ServerController", $"Server recived new message: \"{message}\" from \"{sender}\"");
            ServerListenerController.OnMessageRecived += (sender, message, metadata) => ProcessClientMessage(sender, message, metadata);

            ServerListenerController.OnClientConnected += (client) => Logger.WriteLine("ServerController", $"New client connected: {client}");

            ServerListenerController.OnClientDisconnected += (client, _) => RoomsController.RemoveClientByAddress(client);

            //ServerListenerController.OnClientConnected += (client) => SendHelloMessage(client);
            ServerListenerController.StartServer(null, ConfigController.Port);
        }

        static void SendHelloMessage(string client)
        {
            ServerListenerController.SendMessage(client, "HelloMessageFromServer", new Dictionary<object, object>() {
                { ServerHelloMessageMetadataTypes.ServerType, ServerTypes.MultiRoom },
            });
        }

        static void ProcessClientMessage(string client, string message, Dictionary<object, object> metadata)
        {
            try
            {
                ProcessClientMessageInternal(client, message, metadata);
            } catch (Exception ex)
            {
                Console.WriteLine($"{ex}");
            }
        }

        static void ProcessClientMessageInternal(string client, string message, Dictionary<object, object> metadata)
        {
            switch (message)
            {
                case "HelloMessageFromClient":
                    ProcessClientHelloMessage(client, metadata);
                    break;
                case "GetRooms":
                    ProcessGetRoomsRequest(client);
                    break;
                case "ConnectToRoom":
                    ProcessConnectToRoomRequest(client, metadata);
                    break;
                case "ContentServerRequest":
                    ProcessContentServerRequest(client);
                    break;
                case "ContentClientResponse":
                    ProcessContentClientResponse(metadata);
                    break;
                case "SetRoomStage":
                    ProcessSetRoomStageRequest(client, metadata);
                    break;
                default:
                    OnUnknownMessageRecived(client, message);
                    break;
            }
        }

        static void ProcessClientHelloMessage(string client, Dictionary<object, object> metadata)
        {
            if (metadata[ClientHelloMessageMetadataTypes.ApiVersion].ToString() != ApiVersion)
            {
                ServerListenerController.DisconnectClient(client, MessageStatus.AuthFailure, "Wrong API version");
                return;
            }

            SendHelloMessage(client);
        }

        static void ProcessGetRoomsRequest(string client)
        {
            ServerListenerController.SendMessage(client, "RoomsList", new Dictionary<object, object>()
            {
                { RoomsListMetadataTypes.Rooms, new List<VLCNotAloneShared.POCO.Room>(RoomsController.ActiveRooms.Select(x => x.ToShared())) }
            });
        }

        static void ProcessConnectToRoomRequest(string client, Dictionary<object, object> metadata)
        {
            if (RoomsController.TryAuthClientInRoom(new ClientPOCO { Address = client, Id = (int) metadata[ConnectToRoomMessageMetadataTypes.ClientId], Username = metadata[ConnectToRoomMessageMetadataTypes.Username].ToString() }, metadata[ConnectToRoomMessageMetadataTypes.RoomName].ToString(), metadata[ConnectToRoomMessageMetadataTypes.Password].ToString()))
                ServerListenerController.SendMessage(client, "SuccessConnectToRoom");
            else
                ServerListenerController.SendMessage(client, "FailConnectToRoom");
        }

        static void ProcessContentServerRequest(string client) => RoomsController.ProcessRequestRoomContent(client);

        static void ProcessContentClientResponse(Dictionary<object, object> metadata)
        {
            ServerListenerController.SendMessage(RoomsController.FindClientById((int)metadata[ContentClientResponseMetadataTypes.ReciverId]).Address, "ContentSync", new Dictionary<object, object>()
            {
                {ContentSyncMetadataTypes.Filename,  metadata[ContentClientResponseMetadataTypes.Filename]},
                {ContentSyncMetadataTypes.Mode, metadata[ContentClientResponseMetadataTypes.Mode]},
                {ContentSyncMetadataTypes.Position, metadata[ContentClientResponseMetadataTypes.Position]}
            });
        }

        static void ProcessSetRoomStageRequest(string client, Dictionary<object, object> metadata) => RoomsController.UpdateClientRoomStage(client, metadata);

        public static void RequestContentFromClient(string client, int requesterId) => ServerListenerController.SendMessage(client, "ContentClientRequest", new Dictionary<object, object>() {{ ContentClientRequestMetadataTypes.RequesterId, requesterId }});
        public static void SendRoomStage(string client, Dictionary<object, object> args) => ServerListenerController.SendMessage(client, "SetRoomStage", args);

        public static void NoticeClientConnected(string client, string connectedClientUsername) => ServerListenerController.SendMessage(client, "ClientConnectedNotice", new Dictionary<object, object>() { { ClientConnectedNoticeMetadataTypes.Username, connectedClientUsername } });
#warning Link Disconnect Notice
        public static void NoticeClientDisconnected(string client, string disconnectedClientUsername) => ServerListenerController.SendMessage(client, "ClientDisconnectedNotice", new Dictionary<object, object>() { { ClientDisconnectedNoticeMetadataTypes.Username, disconnectedClientUsername } });

        static void OnUnknownMessageRecived(string client, string message)
        {
            Logger.WriteLine("ServerController", "Unknown message recived from client \"{client}\"");
            ServerListenerController.DisconnectClient(client, MessageStatus.Failure, "Unknown message");
        }
    }
}
