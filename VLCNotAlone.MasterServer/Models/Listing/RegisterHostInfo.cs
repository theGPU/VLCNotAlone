using System.Net;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.MasterServer.Models.Listing
{
    public class RegisterHostInfo : BaseHostInfo
    {
        public Guid Passport { get; set; }

        public HostInfo ToHostInfo()
        {
            return new HostInfo
            {
                ID = ID,
                Passport = Passport,
                Name = Name,
                Port = Port,
                IsPublic = IsPublic,
                ProtocolVersion = ProtocolVersion,
                ClientsCount = ClientsCount,
                RoomsCount = RoomsCount
            };
        }
    }
}
