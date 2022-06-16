using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAloneMultiRoomServer.POCO;
using VLCNotAloneMultiRoomServer.Utils;
using VLCNotAloneShared;
using VLCNotAloneShared.Enums;
using WatsonTcp;

namespace VLCNotAloneMultiRoomServer.Controllers
{
    internal static class ServerController
    {
        public static void Init()
        {
            ServerListenerController.OnServerStarted += () => Logger.WriteLine("ServerController", "Server started");

            ServerListenerController.OnMessageRecived += (sender, message, metadata) => Logger.WriteLine("ServerController", $"Server recived new message: \"{message}\" from \"{sender}\"");
            ServerListenerController.OnMessageRecived += (sender, message, metadata) => ProcessClientMessage(sender, message, metadata);

            ServerListenerController.OnClientConnected += (client) => Logger.WriteLine("ServerController", $"New client connected: {client}");

            ServerListenerController.OnClientDisconnected += (client, _) => RoomsController.OnClientDisconnected(client);

            ServerListenerController.StartServer(ConfigController.Host, ConfigController.Port);
        }

        static void ProcessClientMessage(string client, string message, Dictionary<object, object> metadata)
        {
            try
            {
                ProcessClientMessageInternal(client, message, metadata);
            } catch (Exception ex)
            {
                ServerListenerController.DisconnectClient(client, MessageStatus.Failure, "Unknown error");
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
                case "GetRoomClients":
                    ProcessGetRoomClientsRequest(client);
                    break;
                case "Ping":
                    break;
                default:
                    OnUnknownMessageRecived(client, message);
                    break;
            }
        }

        static void ProcessClientHelloMessage(string client, Dictionary<object, object> metadata)
        {
            if (metadata[Enum.GetName(ClientHelloMessageMetadataTypes.ApiVersion)].ToString() != VersionInfo.ApiVersion)
            {
                ServerListenerController.DisconnectClient(client, MessageStatus.AuthFailure, "Wrong API version");
                return;
            }

            SendHelloMessage(client);
        }

        static void SendHelloMessage(string client)
        {
            ServerListenerController.SendMessage(client, "HelloMessageFromServer", new Dictionary<object, object>() {
                { ServerHelloMessageMetadataTypes.ServerType, ServerTypes.MultiRoom },
            });
        }

        static void ProcessGetRoomsRequest(string client)
        {
            ServerListenerController.SendMessage(client, "RoomsList", new Dictionary<object, object>()
            {
                { RoomsListMetadataTypes.Rooms, new List<VLCNotAloneShared.POCO.Room>(RoomsController.ActiveRooms.Select(x => x.ToShared()).OrderBy(x => x.Name)) }
            });
        }

        static void ProcessConnectToRoomRequest(string client, Dictionary<object, object> metadata)
        {
            if (RoomsController.TryAuthClientInRoom(new ClientPOCO { Address = client, Id = (int) (long) metadata[Enum.GetName(ConnectToRoomMessageMetadataTypes.ClientId)], Username = metadata[Enum.GetName(ConnectToRoomMessageMetadataTypes.Username)].ToString() }, metadata[Enum.GetName(ConnectToRoomMessageMetadataTypes.RoomName)].ToString(), metadata[Enum.GetName(ConnectToRoomMessageMetadataTypes.Password)].ToString()))
                ServerListenerController.SendMessage(client, "SuccessConnectToRoom");
            else
                ServerListenerController.SendMessage(client, "FailConnectToRoom");
        }

        static void ProcessContentServerRequest(string client) => RoomsController.ProcessRequestRoomContent(client);

        static void ProcessContentClientResponse(Dictionary<object, object> metadata)
        {
            ServerListenerController.SendMessage(RoomsController.FindClientById((int)(long)metadata[Enum.GetName(ContentClientResponseMetadataTypes.ReciverId)]).Address, "ContentSync", new Dictionary<object, object>()
            {
                {ContentSyncMetadataTypes.Filename,  metadata[Enum.GetName(ContentClientResponseMetadataTypes.Filename)]},
                {ContentSyncMetadataTypes.Mode, metadata[Enum.GetName(ContentClientResponseMetadataTypes.Mode)]},
                {ContentSyncMetadataTypes.Position, metadata[Enum.GetName(ContentClientResponseMetadataTypes.Position)]}
            });
        }

        static void ProcessSetRoomStageRequest(string client, Dictionary<object, object> metadata) => RoomsController.UpdateClientRoomStage(client, metadata);

        public static void RequestContentFromClient(string client, int requesterId) => ServerListenerController.SendMessage(client, "ContentClientRequest", new Dictionary<object, object>() {{ ContentClientRequestMetadataTypes.RequesterId, requesterId }});
        public static void SendRoomStage(string client, Dictionary<object, object> args) => ServerListenerController.SendMessage(client, "SetRoomStage", args);

        private static void ProcessGetRoomClientsRequest(string client)
        {
            var room = RoomsController.FindRoomWithClientByAddress(client);
            ServerListenerController.SendMessage(client, "RoomClients", new Dictionary<object, object>() { { RoomClientsResponseMetadataTypes.Clients, room.AuthedClients.Select(x => x.Username).OrderBy(x => x) } });
        }

        public static void NoticeClientConnected(string client, string connectedClientUsername) => ServerListenerController.SendMessage(client, "ClientConnectedNotice", new Dictionary<object, object>() { { ClientConnectedNoticeMetadataTypes.Username, connectedClientUsername } });
        public static void NoticeClientDisconnected(string client, string disconnectedClientUsername) => ServerListenerController.SendMessage(client, "ClientDisconnectedNotice", new Dictionary<object, object>() { { ClientDisconnectedNoticeMetadataTypes.Username, disconnectedClientUsername } });

        static void OnUnknownMessageRecived(string client, string message)
        {
            Logger.WriteLine("ServerController", $"Unknown message recived from client \"{client}\"");
            ServerListenerController.DisconnectClient(client, MessageStatus.Failure, "Unknown message");
        }
    }
}
