using Newtonsoft.Json;
using VLCNotAlone.Server.Models.Room;
using VLCNotAlone.Shared.Models.Room;

namespace VLCNotAlone.Server.Services
{
    public class RoomsService
    {
        ILogger<RoomsService> _logger;

#warning Move to config
        const string RoomsFilePath = "Rooms.txt";
        private readonly object saveLocker = new object();

        private readonly Dictionary<int, RoomData> _rooms = new Dictionary<int, RoomData>();
        public ListingRoomInfo[] RoomsListing => _rooms.Select(x => x.Value.ToListingInfo(x.Key)).ToArray();
        public int RoomsCount => _rooms.Count;
        private int nextRoomId = -1;
        private int NextRoomId => Interlocked.Increment(ref nextRoomId);

        public RoomsService(ILogger<RoomsService> logger)
        {
            _logger = logger;
            Init();
        }

        private void Init()
        {
            _logger.LogInformation("Init");
            LoadRooms();
        }

        private void LoadRooms()
        {
            if (!File.Exists(RoomsFilePath))
                File.WriteAllText(RoomsFilePath, "[]");

            _logger.LogInformation("Load rooms");
            var roomsJson = File.ReadAllText(RoomsFilePath);
            var rooms = JsonConvert.DeserializeObject<List<RoomData>>(roomsJson)!;
            rooms.ForEach(x => _rooms.Add(NextRoomId, x));
            _logger.LogInformation("Loaded {Count} rooms from {RoomsFilePath}", _rooms.Count, RoomsFilePath);
        }

        private void SaveRooms()
        {
            lock (saveLocker)
            {
                File.WriteAllText(RoomsFilePath, JsonConvert.SerializeObject(_rooms.Values, Formatting.Indented));
            }
        }
    }
}
