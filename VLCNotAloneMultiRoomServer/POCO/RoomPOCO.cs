using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAloneMultiRoomServer.POCO
{
    public class RoomPOCO
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool HasPassword { get; set; }
        public string Password { get; set; }

        [JsonIgnore]
        public List<ClientPOCO> AuthedClients { get; set; } = new List<ClientPOCO>();

        public static explicit operator VLCNotAloneShared.POCO.Room(RoomPOCO room) => new VLCNotAloneShared.POCO.Room() { Name = room.Name, HasPassword = room.HasPassword };
    }
}
