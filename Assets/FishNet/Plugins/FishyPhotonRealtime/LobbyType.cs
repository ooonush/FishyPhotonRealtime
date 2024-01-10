using System;

namespace FishNet.Transporting.PhotonRealtime
{
    /// <summary>Types of lobbies define their behaviour and capabilities. Check each value for details.</summary>
    /// <remarks>Values of this enum must be matched by the server.</remarks>
    [Serializable]
    public enum LobbyType :byte
    {
        /// <summary>Standard type and behaviour: While joined to this lobby clients get room-lists and JoinRandomRoom can use a simple filter to match properties (perfectly).</summary>
        Default = 0,
        /// <summary>This lobby type lists rooms like Default but JoinRandom has a parameter for SQL-like "where" clauses for filtering. This allows bigger, less, or and and combinations.</summary>
        SqlLobby = 2,
        /// <summary>This lobby does not send lists of games. It is only used for OpJoinRandomRoom. It keeps rooms available for a while when there are only inactive users left.</summary>
        AsyncRandomLobby = 3
    }
}