using Newtonsoft.Json;
using VLCNotAlone.Shared.Models.Room;

namespace VLCNotAlone.Server.Models.Room
{
    public class RoomData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
        public HashSet<string> ConnectedClients { get; set; }
        public HashSet<string> PasswordEnteringClients { get; set; }

        public bool HasPassword => !string.IsNullOrEmpty(Password);

        public ListingRoomInfo ToListingInfo(int id) => new ListingRoomInfo(id, Name, Description, HasPassword, ConnectedClients.Count + PasswordEnteringClients.Count);
    }
}
