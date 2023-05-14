using System;
using System.ComponentModel;

namespace VLCNotAlone.Shared.Models.Configs
{
    public class NetworkConfig
    {
        [Description("The name of this server. Max 250 characters.")]
        public string ServerName { get; set; } = "VLCNotAlone Server";

        [Description("A long description of the server to display when selected in the server browser. Max 500 characters.")]
        public string Description { get; set; } = "";

        [Description("Internal IP Address for the traffic. If not specified then Any (0.0.0.0) address will be used. Requires a restart to take effect.")]
        public string Password { get; set; } = "";

        [Description("Port for server traffic. TCP only. Requires a restart to take effect.")]
        public ushort ServerPort { get; set; } = 7133;

        [Description("Internal IP Address for the traffic. If not specified then Any (0.0.0.0) address will be used. Requires a restart to take effect.")]
        public string IPAddress { get; set; } = "Any";

        [Description("Server unique identifier (generates automatically). Should be cleaned-up for new server.")]
        public Guid ID { get; set; } = Guid.NewGuid();

        [Description("Server passport which used to confirm it owns ID. Protects from server substitution. Should be cleaned-up for new server."), Browsable(false)]
        public Guid Passport { get; set; } = Guid.NewGuid();

        [Description("Universal Plug and Play (UPnP) allows to auto-configure external traffic mapping for servers behind NAT")]
        public bool UPnPEnabled { get; set; } = true;
    }
}
