using System.Net;

namespace VLCNotAlone.Shared.Models
{
    public class ListingHostInfo : BaseHostInfo
    {
        public IPAddress IpAddress { get; set; }
        public bool IsOfficial { get; set; }
    }
}
