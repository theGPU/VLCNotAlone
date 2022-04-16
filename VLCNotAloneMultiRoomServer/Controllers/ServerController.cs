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
            ServerListenerController.OnClientConnected += (client) => Logger.WriteLine("ServerController", $"New client connected: {client}");
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
            switch (message)
            {
                case "HelloMessageFromClient":
                    ProcessClientHelloMessage(client, message, metadata);
                    break;
                case "GetRooms":
                    ProcessGetRoomsRequest(client);
                    break;
                case "ConnectToRoom":
                    ProcessConnectToRoomRequest(client, metadata);
                    break;
                case "WhatTime":
                    ProcessWhatTimeRequest(client);
                    break;
                default:
                    OnUnknownMessageRecived(client, message);
                    break;
            }
        }

        static void ProcessClientHelloMessage(string client, string message, Dictionary<object, object> metadata)
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
                { RoomsListMetadataTypes.Rooms, new List<VLCNotAloneShared.POCO.Room>(RoomsController.ActiveRooms.Cast<VLCNotAloneShared.POCO.Room>()) }
            });
        }

        static void ProcessConnectToRoomRequest(string client, Dictionary<object, object> metadata)
        {
            if (RoomsController.TryAuthClientInRoom(new ClientPOCO { Address = client, Id = (int) metadata[ConnectToRoomMessageMetadataTypes.ClientId], Username = metadata[ConnectToRoomMessageMetadataTypes.Username].ToString() }, metadata[ConnectToRoomMessageMetadataTypes.RoomName].ToString(), metadata[ConnectToRoomMessageMetadataTypes.Password].ToString()))
                ServerListenerController.SendMessage(client, "SuccessConnectToRoom");
            else
                ServerListenerController.SendMessage(client, "FailConnectToRoom");
        }

        static void ProcessWhatTimeRequest(string client)
        {

        }

        static void OnUnknownMessageRecived(string client, string message)
        {
            Logger.WriteLine("ServerController", "Unknown message recived from client \"{client}\"");
            ServerListenerController.DisconnectClient(client, MessageStatus.Failure, "Unknown message");
        }
    }
}
