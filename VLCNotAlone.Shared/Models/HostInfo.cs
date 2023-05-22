using System;

namespace VLCNotAlone.Shared.Models
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
                Protocol = Protocol,
                IsPublic = IsPublic,
                ServerVersion = ServerVersion,
                ClientsCount = ClientsCount,
                RoomsCount = RoomsCount
            };
        }
    }
}
