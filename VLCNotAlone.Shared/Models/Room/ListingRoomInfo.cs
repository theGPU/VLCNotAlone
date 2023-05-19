using System;
using System.Collections.Generic;
using System.Text;

namespace VLCNotAlone.Shared.Models.Room
{
    public class ListingRoomInfo
    {        
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasPassword { get; set; }
        public int Clients { get; set; }

        public ListingRoomInfo(int id, string name, string description, bool hasPassword, int clients)
        {
            Id = id;
            Name = name;
            Description = description;
            HasPassword = hasPassword;
            Clients = clients;
        }
    }
}
