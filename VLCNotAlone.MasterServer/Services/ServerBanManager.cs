using System.Net;
using VLCNotAlone.MasterServer.Models.Listing;

namespace VLCNotAlone.MasterServer.Services
{
    public class ServerBanManager
    {
        const string BanListPath = "BanList.txt";

        public ServerBanManager() { }

        public bool IsServerBanned(IPAddress senderIPAddress, RegisterHostInfo info)
        {
            var banList = File.ReadAllLines(BanListPath);
            if (banList.Any(x => x == senderIPAddress.ToString() || x == info.ID.ToString() || x == info.Passport.ToString() || x == info.Name))
                return true;

            return false;
        }
    }
}
