using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VLCNotAloneServer
{
    internal class ConnectedClientPOCO
    {
        public string UserName { get; set; }
        public string Nickname { get; set; }
        public string Id { get; set; }
        public string APIVersion { get; set; }
        public Socket Socket { get; set; }
    }
}
