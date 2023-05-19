using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VLCNotAlone.Shared.Models.Configs
{
    public class NetworkConfig : IRemoteConfigModel
    {
        [Description("The name of this server. Max 250 characters."), Required, MinLength(1), MaxLength(250)]
        public string ServerName { get; set; } = "VLCNotAlone Server";

        [Description("A long description of the server to display when selected in the server browser. Max 500 characters."), Required, MaxLength(500)]
        public string Description { get; set; } = "";

        [Description("Internal IP Address for the traffic. If not specified then Any (0.0.0.0) address will be used. Requires a restart to take effect."), Required, MaxLength(16)]
        public string Password { get; set; } = "";

        [Description("Port for server traffic. TCP only. Requires a restart to take effect."), Required]
        public ushort ServerPort { get; set; } = 7133;

        [Description("Internal IP Address for the traffic. If not specified then Any (0.0.0.0) address will be used. Requires a restart to take effect."), Required]
        public string IPAddress { get; set; } = "Any";

        [Description("Server unique identifier (generates automatically). Should be cleaned-up for new server."), Required]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Description("Server passport which used to confirm it owns ID. Protects from server substitution. Should be cleaned-up for new server."), Browsable(false)]
        public Guid Passport { get; set; } = Guid.NewGuid();

        [Description("Universal Plug and Play (UPnP) allows to auto-configure external traffic mapping for servers behind NAT"), Required]
        public bool UPnPEnabled { get; set; } = true;

        public bool IsValid(out List<ValidationResult> results)
        {
            results = new List<ValidationResult>();
            if (Validator.TryValidateObject(this, new ValidationContext(this), results, true))
            {
                return true;
            } 
            else
            {
                return false;
            }
        }
    }
}
