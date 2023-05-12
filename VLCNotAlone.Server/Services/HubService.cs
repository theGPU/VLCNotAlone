using Microsoft.AspNetCore.SignalR;

namespace VLCNotAlone.Server.Services
{
    public class HubService : Hub
    {
        public async Task Send(string username, string message)
        {
            await this.Clients.All.SendAsync("Receive", username, message);
        }
    }
}
