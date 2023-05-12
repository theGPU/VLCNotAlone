using System.Net;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.MasterServer.Models.Listing
{
    public class HostInfo : ListingHostInfo
    {
        public Guid Passport { get; set; }
        public bool CheckSucceed { get; set; }
        public DateTime LastUpdated { get; set; }

        public ListingHostInfo ToListingHostInfo()
        {
            return new ListingHostInfo
            {
                ID = ID,
                Name = Name,
                IpAddress = IpAddress,
                Port = Port,
                IsPublic = IsPublic,
                ProtocolVersion = ProtocolVersion,
                ClientsCount = ClientsCount,
                RoomsCount = RoomsCount
            };
        }
    }
}
