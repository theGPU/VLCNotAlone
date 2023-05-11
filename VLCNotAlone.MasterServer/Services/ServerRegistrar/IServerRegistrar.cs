using System.Net;
using VLCNotAlone.MasterServer.Models.Listing;
using VLCNotAlone.Shared;

namespace VLCNotAlone.MasterServer.Services.ServerRegistrar
{
    public interface IServerRegistrar
    {
        MasterServerError ProcessServerRegister(IPAddress? ip, RegisterHostInfo registerHostInfo);
    }
}
