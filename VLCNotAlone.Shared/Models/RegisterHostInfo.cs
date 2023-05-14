using System;
using System.Net;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.Shared.Models
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
                ServerVersion = ServerVersion,
                ClientsCount = ClientsCount,
                RoomsCount = RoomsCount
            };
        }
    }
}
