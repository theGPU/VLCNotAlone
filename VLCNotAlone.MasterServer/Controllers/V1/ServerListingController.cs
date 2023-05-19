using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VLCNotAlone.MasterServer.Services.ServerRegistrar;
using VLCNotAlone.MasterServer.Services.ServerRegistry;
using VLCNotAlone.Shared;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.MasterServer.Controllers.V1
{
    [Tags("Server Listing Controller")]
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ServerListingController : ControllerBase
    {
        private readonly IServerRegistry _serverRegistry;
        private readonly IServerRegistrar _serverRegistrar;

        public ServerListingController(IServerRegistry serverRegistry, IServerRegistrar serverRegistrar)
        {
            _serverRegistry = serverRegistry;
            _serverRegistrar = serverRegistrar;
        }

        [HttpGet]
        public string Get() => SerializeHosts(_serverRegistry.GetPublicServers());

        [HttpGet("official")]
        public string GetOfficial() => SerializeHosts(_serverRegistry.GetOfficialServers());

        [HttpPost("registerserver")] //[FromServices]...
        public MasterServerError RegisterServer([FromBody] RegisterHostInfo registerHostInfo) => _serverRegistrar.ProcessServerRegister(HttpContext.Connection.RemoteIpAddress, registerHostInfo);

        private string SerializeHosts(IEnumerable<HostInfo> hosts)
        {
            var hostsToSerialize = hosts.Select(x => x.ToListingHostInfo());
            return JsonConvert.SerializeObject(hostsToSerialize, Constants.MasterServerJsonSerializerSettings);
        }
    }
}
