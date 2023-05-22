using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.Models
{
    public class ServerListItem
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IPAddress IpAddress { get; set; }
        public ushort Port { get; set; }
        public ServerProtocol Protocol { get; set; }
        public string Version { get; set; }
        public bool IsFavorite { get; set; }
        public int ClientsCount { get; set; }
        public int RoomsCount { get; set; }
        public string Ping { get; set; }

        public static ServerListItem FromListingHostInfo(ListingHostInfo listingHostInfo)
        {
            var result = new ServerListItem()
            {
                ID = listingHostInfo.ID,
                Name = listingHostInfo.Name,
                Description = listingHostInfo.Description,
                IpAddress = listingHostInfo.IpAddress,
                Port = listingHostInfo.Port,
                Protocol = listingHostInfo.Protocol,
                Version = listingHostInfo.ServerVersion,
                IsFavorite = false,
                ClientsCount = listingHostInfo.ClientsCount,
                RoomsCount = listingHostInfo.RoomsCount,
                Ping = "-"
            };
            return result;
        }
    }
}
