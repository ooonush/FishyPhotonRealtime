using System;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    [Serializable]
    public sealed class ConnectionData
    {
        /// <summary>
        /// Type (and behaviour) of the lobby.
        /// </summary>
        /// <remarks>
        /// An empty or null Name always points to the "default lobby" as special case.
        /// </remarks>
        public LobbyType LobbyType;
        /// <summary>
        /// Name of the lobby. Default: null, pointing to the "default lobby".
        /// </summary>
        /// <remarks>
        /// If Name is null or empty, a TypedLobby will point to the "default lobby". This ignores the Type value and always acts as  <see cref="LobbyType.Default"/>.
        /// </remarks>
        public string LobbyName;
        /// <summary>The name of the room to create. If null, the server generates a unique name. If not null, it must be unique and new or will cause an error.</summary>
        public string RoomName;
        /// <summary>The MatchmakingMode affects how rooms get filled. By default, the server fills rooms.</summary>
        public MatchmakingMode MatchmakingMode;
        /// <summary>SQL query to filter room matches. For default-typed lobbies, use ExpectedCustomRoomProperties instead.</summary>
        public string MatchmakingSqlLobbyFilter;
    }
}