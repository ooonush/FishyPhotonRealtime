﻿using ExitGames.Client.Photon;

namespace FishNet.Transporting.PhotonRealtime
{
    public class JoinRandomRoomData
    {
        public string[] ExpectedUsers;
        public Hashtable CustomRoomProperties;
        public string SqlLobbyFilter;
        public object Ticket;
    }
}