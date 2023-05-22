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
    public sealed class MasterServerRestApiClient : IDisposable
    {
        private readonly HttpClient client = new HttpClient();

        public MasterServerRestApiClient() { }

        public async Task<IEnumerable<ListingHostInfo>> TryRequestHostList()
        {
            var response = await RetrieveServerList();
            var hostList = JsonConvert.DeserializeObject<IEnumerable<ListingHostInfo>>(response, Constants.MasterServerJsonSerializerSettings);
            return hostList;
        }

        public async Task<IEnumerable<ListingHostInfo>> TryRequestOfficialHostList()
        {
            var response = await RetrieveServerList("/official");
            var hostList = JsonConvert.DeserializeObject<IEnumerable<ListingHostInfo>>(response, Constants.MasterServerJsonSerializerSettings);
            return hostList;
        }

        private async Task<string> RetrieveServerList(string suffix = "")
        {
            var url = $"{Constants.MasterServerUrl}/api/v{Constants.MasterServerProtocolVersion}/serverListing{suffix}";
            try
            {
                var response = await client.GetStringAsync(url);
                return response;
            } catch (Exception ex)
            {
                #warning Add log here
                return "[]";
            }
        }

        public async Task<MasterServerError> RegisterServer(RegisterHostInfo hostInfo)
        {
            var url = $"{Constants.MasterServerUrl}/api/v{Constants.MasterServerProtocolVersion}/serverListing/registerServer";
            try
            {
                var response = await client.PostAsync(url, new StringContent(JsonConvert.SerializeObject(hostInfo, Constants.MasterServerJsonSerializerSettings), Encoding.UTF8, "application/json"));
                var resp = await response.Content.ReadAsStringAsync();
                if (int.TryParse(resp, out int statusCode))
                {
                    return (MasterServerError)statusCode;
                } else
                {
                    #warning Add log here
                    return MasterServerError.RequestError;
                }
            } catch (Exception ex)
            {
                #warning Add log here
                return MasterServerError.RequestError;
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
