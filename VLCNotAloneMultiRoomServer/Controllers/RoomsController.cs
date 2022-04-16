using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLCNotAloneMultiRoomServer.POCO;

namespace VLCNotAloneMultiRoomServer.Controllers
{
    internal static class RoomsController
    {
        public static List<RoomPOCO> Rooms = new List<RoomPOCO>();
        public static RoomPOCO[] ActiveRooms => Rooms.Where(x => x.IsActive).ToArray();

        public static bool TryAuthClientInRoom(ClientPOCO client, string roomName, string password)
        {
            var room = ActiveRooms.FirstOrDefault(x => x.Name == roomName);
            if (!room?.HasPassword == false || room?.Password == password)
            {
                RemoveClientByAddress(client.Address);
                RegisterClientInRoom(client, room);
                return true;
            }
            
            return false;
        }

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
        }

        public static bool ProcessRequestRoomContent(string client)
        {
            var room = FindRoomWithClientByAddress(client);
            if (room == null)
                return false;

            var clientInst = room.AuthedClients.Find(x => x.Address == client);
            if (room.AuthedClients[0] == clientInst)
                return false;

            return true;
        }
    }
}
