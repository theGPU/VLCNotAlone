using System.Net;
using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.MasterServer.Services.ServerRegistrar
{
    public interface IServerRegistrar
    {
        MasterServerError ProcessServerRegister(IPAddress? ip, RegisterHostInfo registerHostInfo);
    }
}
