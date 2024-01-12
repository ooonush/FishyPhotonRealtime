using System;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    /// <summary>Refers to a specific lobby on the server.</summary>
    /// <remarks>
    /// Name and Type combined are the unique identifier for a lobby.<br/>
    /// The server will create lobbies "on demand", so no registration or setup is required.<br/>
    /// An empty or null Name always points to the "default lobby" as special case.
    /// </remarks>
    [Serializable]
    public struct TypedLobby
    {
        /// <summary>
        /// Name of the lobby. Default: null, pointing to the "default lobby".
        /// </summary>
        /// <remarks>
        /// If Name is null or empty, a TypedLobby will point to the "default lobby". This ignores the Type value and always acts as  <see cref="LobbyType.Default"/>.
        /// </remarks>
        [Tooltip("Name of the lobby. Default: null, pointing to the \"default lobby\".")]
        public string Name;
        /// <summary>
        /// Type (and behaviour) of the lobby.
        /// </summary>
        /// <remarks>
        /// An empty or null Name always points to the "default lobby" as special case.
        /// </remarks>
        [Tooltip("Type (and behaviour) of the lobby.")]
        public LobbyType Type;
        /// <summary>
        /// Returns whether or not this instance points to the "default lobby" (<see cref="TypedLobby.Default"/>).
        /// </summary>
        /// <remarks>
        /// This comes up to checking if the Name is null or empty.
        /// <see cref="LobbyType.Default"/> is not the same thing as the "default lobby" (<see cref="TypedLobby.Default"/>).
        /// </remarks>
        public bool IsDefault { get { return string.IsNullOrEmpty(Name); } }
        /// <summary>
        /// Sets Name and Type of the new instance. Make sure name is not empty or null, as that always points to the "default lobby" (<see cref="TypedLobby.Default"/>).
        /// </summary>
        /// <param name="name">Some string to identify a lobby.</param>
        /// <param name="type">The type of a lobby defines it's capabilities and behaviour.</param>
        public TypedLobby(string name, LobbyType type)
        {
            Name = name;
            Type = type;
        }
        public override string ToString()
        {
            return $"lobby '{Name}'[{Type}]";
        }
    }
}