using System;
using System.ComponentModel.DataAnnotations;

namespace VLCNotAlone.Shared.Models
{
    public class BaseHostInfo
    {
        public Guid ID { get; set; }
        [MinLength(1), MaxLength(250)] public string Name { get; set; }
        [MinLength(1), MaxLength(500)] public string Description { get; set; }

        public ushort Port { get; set; }

        public bool IsPublic { get; set; }
        public bool HasPassword { get; set; }
        [MinLength(5), MaxLength(10)]
        public string ServerVersion { get; set; }
        public int ClientsCount { get; set; }
        public int RoomsCount { get; set; }

        public RegisterHostInfo ToRegisterHostInfo(Guid passport)
        {
            var registerHostInfo = new RegisterHostInfo() {
                ID = ID,
                Passport = passport,
                Name = Name, 
                Description = Description, 
                Port = Port, 
                IsPublic = IsPublic, 
                HasPassword = HasPassword, 
                ServerVersion = ServerVersion, 
                ClientsCount = ClientsCount, 
                RoomsCount = RoomsCount 
            };
            return registerHostInfo;
        }
    }
}
