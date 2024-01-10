using System;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    /// <summary>Wraps up common room properties needed when you create rooms. Read the individual entries for more details.</summary>
    /// <remarks>This directly maps to the fields in the Room class.</remarks>
    [Serializable]
    public class RoomOptions
    {
        /// <summary>Defines if this room is listed in the lobby. If not, it also is not joined randomly.</summary>
        /// <remarks>
        /// A room that is not visible will be excluded from the room lists that are sent to the clients in lobbies.
        /// An invisible room can be joined by name but is excluded from random matchmaking.
        ///
        /// Use this to "hide" a room and simulate "private rooms". Players can exchange a roomname and create it
        /// invisble to avoid anyone else joining it.
        /// </remarks>
        [Tooltip("Defines if this room is listed in the lobby. If not, it also is not joined randomly.")]
        public bool IsVisible = true;

        /// <summary>Defines if this room can be joined at all.</summary>
        /// <remarks>
        /// If a room is closed, no player can join this. As example this makes sense when 3 of 4 possible players
        /// start their gameplay early and don't want anyone to join during the game.
        /// The room can still be listed in the lobby (set isVisible to control lobby-visibility).
        /// </remarks>
        [Tooltip("Defines if this room can be joined at all.")]
        public bool IsOpen = true;

        [Tooltip("Max number of players that can be in the room at any time. 0 means \"no limit\".")]
        public int MaxPlayers;

        [Tooltip("Time To Live (TTL) for an 'actor' in a room. If a client disconnects, this actor is inactive first and removed after this timeout. In milliseconds.")]
        public int PlayerTtl;

        [Tooltip("Time To Live (TTL) for a room when the last player leaves. Keeps room in memory for case a player re-joins soon. In milliseconds.")]
        public int EmptyRoomTtl;

        /// <remarks>
        /// This makes sense when in rooms where players can't place items in the room and just vanish entirely.
        /// When you disable this, the event history can become too long to load if the room stays in use indefinitely.
        /// Default: true. Cleans up the cache and props of leaving users.
        /// </remarks>
        [Tooltip("Removes a user's events and properties from the room when a user leaves.")]
        public bool CleanupCacheOnLeave = true;

        /// <remarks>
        /// Using this makes the client unaware of the other players in a room.
        /// That can save some traffic if you have some server logic that updates players
        /// but it can also limit the client's usability.
        /// </remarks>
        [Tooltip("Tells the server to skip room events for joining and leaving players.")]
        public bool SuppressRoomEvents;

        [Tooltip("Disables events join and leave from the server as well as property broadcasts in a room (to minimize traffic)")]
        public bool SuppressPlayerInfo;

        /// <remarks>
        /// When you set this to true, Photon will publish the UserIds of the players in that room.
        /// In that case, you can use PhotonPlayer.userId, to access any player's userID.
        /// This is useful for FindFriends and to set "expected users" to reserve slots in a room.
        /// </remarks>
        [Tooltip("Defines if the UserIds of players get \"published\" in the room. Useful for FindFriends, if players want to play another game together.")]
        public bool PublishUserId;

        /// <remarks>
        /// When Op SetProperties is setting a key's value to null, the server and clients should remove the key/value from the Custom Properties.
        /// By default, the server keeps the keys (and null values) and sends them to joining players.
        ///
        /// Important: Only when SetProperties does a "broadcast", the change (key, value = null) is sent to clients to update accordingly.
        /// This applies to Custom Properties for rooms and actors/players.
        /// </remarks>
        [Tooltip("Optionally, properties get deleted, when null gets assigned as value. Defaults to off / false.")]
        public bool DeleteNullProperties;

        /// <remarks>
        /// This option is enables by default to fix this scenario:
        ///
        /// 1) On server, room property ABC is set to value FOO, which triggers notifications to all the clients telling them that the property changed.
        /// 2) While that notification is in flight, a client sets the ABC property to value BAR.
        /// 3) Client receives notification from the server and changes it�s local copy of ABC to FOO.
        /// 4) Server receives the set operation and changes the official value of ABC to BAR, but never notifies the client that sent the set operation that the value is now BAR.
        ///
        /// Without this option, the client that set the value to BAR never hears from the server that the official copy has been updated to BAR, and thus gets stuck with a value of FOO.
        /// </remarks>
        [Tooltip("By default, property changes are sent back to the client that's setting them to avoid de-sync when properties are set concurrently.")]
        public bool BroadcastPropsChangeToAll = true;

#if SERVERSDK
        public bool CheckUserOnJoin;
#endif

        /// <remarks>
        /// The operation will fail in case of a plugin missmatch returning error code PluginMismatch 32757(0x7FFF - 10).
        /// Setting string[]{} means the client expects no plugin to be setup.
        /// Note: for backwards compatibility null omits any check.
        /// </remarks>
        [Tooltip("Informs the server of the expected plugin setup.")]
        public string[] Plugins;
    }
}