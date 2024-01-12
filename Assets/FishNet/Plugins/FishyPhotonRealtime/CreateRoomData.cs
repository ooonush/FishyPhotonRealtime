using ExitGames.Client.Photon;

namespace FishNet.Transporting.PhotonRealtime
{
    public class CreateRoomData
    {
        public string RoomName;
        public string[] ExpectedUsers;
        public Hashtable CustomRoomProperties;
        public string[] CustomRoomPropertiesForLobby = new string[0];
    }
}