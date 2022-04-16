using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAloneMultiRoomServer.POCO;
using VLCNotAloneShared.Enums;

namespace VLCNotAloneMultiRoomServer.Controllers
{
    internal static class RoomsController
    {
        public static List<RoomPOCO> Rooms = new List<RoomPOCO>();
        public static RoomPOCO[] ActiveRooms => Rooms.Where(x => x.IsActive).ToArray();

        public static void ReloadRooms()
        {
            var loadedRooms = JsonConvert.DeserializeObject<List<RoomPOCO>>(File.ReadAllText("Rooms.json"));
            var roomsToDelete = Rooms.Where(x => !loadedRooms.Any(y => y.Name == x.Name)).ToList();
            var newRooms = loadedRooms.Where(x => !Rooms.Any(y => y.Name == x.Name));

            Rooms.AddRange(newRooms);
#warning link del
            roomsToDelete.ForEach(x => Rooms.Remove(x));
        }

        public static bool TryAuthClientInRoom(ClientPOCO client, string roomName, string password)
        {
            var room = ActiveRooms.FirstOrDefault(x => x.Name == roomName);
            if (!room?.HasPassword == false || room?.Password == password)
            {
                RemoveClientByAddress(client.Address);
                RegisterClientInRoom(client, room);
                SetRoomStage(room, new Dictionary<object, object>() { { SetRoomStageMetadataTypes.Paused, true } });
                return true;
            }
            
            return false;
        }

        public static void UpdateClientRoomStage(string client, Dictionary<object, object> data) => SetRoomStage(FindRoomWithClientByAddress(client), data);
        public static void SetRoomStage(RoomPOCO room, Dictionary<object, object> data) => room.AuthedClients.ForEach(x => ServerController.SendRoomStage(x.Address, data));

        public static ClientPOCO FindClientById(int id) => ActiveRooms.FirstOrDefault(x => x.AuthedClients.Any(x => x.Id == id))?.AuthedClients.First(x => x.Id == id);

        public static RoomPOCO FindRoomWithClientByAddress(string address) => ActiveRooms.FirstOrDefault(x => x.AuthedClients.Any(x => x.Address == address));
        public static RoomPOCO FindRoomWithClientByUsername(string username) => ActiveRooms.FirstOrDefault(x => x.AuthedClients.Any(x => x.Username == username));
        public static RoomPOCO FindRoomWithClientById(int id) => ActiveRooms.FirstOrDefault(x => x.AuthedClients.Any(x => x.Id == id));

        public static void RemoveClientByAddress(string address) => FindRoomWithClientByAddress(address).AuthedClients.RemoveAll(x => x.Address == address);
        public static void RemoveClientByUsername(string username) => FindRoomWithClientByUsername(username).AuthedClients.RemoveAll(x => x.Username == username);
        public static void RemoveClientById(int id) => FindRoomWithClientById(id).AuthedClients.RemoveAll(x => x.Id == id);

        public static void RegisterClientInRoom(ClientPOCO client, RoomPOCO room)
        {
            RemoveClientByAddress(client.Address);
            room.AuthedClients.Add(client);

            room.AuthedClients.ForEach(x => ServerController.NoticeClientConnected(x.Address, client.Username));
        }

        public static bool ProcessRequestRoomContent(string client)
        {
            var room = FindRoomWithClientByAddress(client);
            if (room == null)
                return false;

            var clientInst = room.AuthedClients.Find(x => x.Address == client);
            if (room.AuthedClients[0] == clientInst)
                return false;

            ServerController.RequestContentFromClient(room.AuthedClients[0].Address, clientInst.Id);
            return true;
        }
    }
}
