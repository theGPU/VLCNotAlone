using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VLCNotAlone.Shared.Models;

namespace VLCNotAlone.Shared.Networking
{
    public sealed class MasterServerClient : IDisposable
    {
        private readonly HttpClient client = new HttpClient();

        public MasterServerClient() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IEnumerable<ListingHostInfo>> TryRequestHostList()
        {
            var response = await client.GetStringAsync($"{Constants.MasterServerUrl}/api/v{Constants.MasterServerProtocolVersion}/serverListing");
            var hostList = JsonConvert.DeserializeObject<IEnumerable<ListingHostInfo>>(response);
            return hostList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<IEnumerable<ListingHostInfo>> TryRequestOfficialHostList()
        {
            var response = await client.GetStringAsync($"{Constants.MasterServerUrl}/api/v{Constants.MasterServerProtocolVersion}/serverListing/official");
            var hostList = JsonConvert.DeserializeObject<IEnumerable<ListingHostInfo>>(response);
            return hostList;
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
