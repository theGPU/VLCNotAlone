using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace VLCNotAlone.MasterServer.Models.Listing
{
    public class BaseHostInfo
    {
        public Guid ID { get; set; }
        [StringLength(128)] public string Name { get; set; }
        [StringLength(4096)] public string Description { get; set; }

        public ushort Port { get; set; }

        public bool IsPublic { get; set; }
        public bool HasPassword { get; set; }
        public int ProtocolVersion { get; set; }
        public int ClientsCount { get; set; }
        public int RoomsCount { get; set; }
    }
}
