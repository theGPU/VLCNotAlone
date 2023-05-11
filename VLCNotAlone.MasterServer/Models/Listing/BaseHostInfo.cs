namespace VLCNotAlone.MasterServer.Models.Listing
{
    public class BaseHostInfo
    {
        public Guid ID { get; set; }
        public string Name { get; set; }

        public ushort Port { get; set; }

        public bool IsPublic { get; set; }
        public int ProtocolVersion { get; set; }
        public int ClientsCount { get; set; }
        public int RoomsCount { get; set; }
    }
}
