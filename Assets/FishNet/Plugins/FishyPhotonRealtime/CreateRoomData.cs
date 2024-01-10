using ExitGames.Client.Photon;

namespace FishNet.Transporting.PhotonRealtime
{
    public class CreateRoomData
    {
        public string[] ExpectedUsers;
        public Hashtable CustomRoomProperties;
        public string[] CustomRoomPropertiesForLobby = new string[0];
    }
}