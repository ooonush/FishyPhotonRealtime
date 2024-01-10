using UnityEngine;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;
using FishNet.Transporting.PhotonRealtime.FishNet.Plugins.FishyPhotonRealtime;

namespace FishNet.Transporting.PhotonRealtime
{
    [Serializable]
    public enum JoinRoomMode : byte
    {
        ByName,
        Random
    }

    public partial class FishyPhotonRealtime : Transport
    {
        private const int KICK_CLIENT_CODE = 2;

        private bool _isInitialized;
        private readonly LoadBalancingClient _client = new LoadBalancingClient();

        private AppSettings AppSettings => PhotonSettings.AppSettings;

        public Room CurrentRoom => _client.CurrentRoom;

        #region Configs

        [SerializeField] public PhotonSettingsAsset PhotonSettings;
        [SerializeField] public RoomOptions RoomOptions;

        /// <summary>
        /// Type (and behaviour) of the lobby.
        /// </summary>
        /// <remarks>
        /// An empty or null Name always points to the "default lobby" as special case.
        /// </remarks>
        [Header("LOBBY")]
        public LobbyType LobbyType;
        /// <summary>
        /// Name of the lobby. Default: null, pointing to the "default lobby".
        /// </summary>
        /// <remarks>
        /// If Name is null or empty, a TypedLobby will point to the "default lobby". This ignores the Type value and always acts as  <see cref="LobbyType.Default"/>.
        /// </remarks>
        public string LobbyName;

        /// <summary>The name of the room to create. If null, the server generates a unique name. If not null, it must be unique and new or will cause an error.</summary>
        [Header("ENTER ROOM OPTIONS")]
        public string RoomName;
        /// <summary>The custom player properties that describe this client / user. Keys must be strings.</summary>
        public Hashtable PlayerProperties;

        /// <summary>The MatchmakingMode affects how rooms get filled. By default, the server fills rooms.</summary>
        [Header("ENTER RANDOM ROOM OPTIONS")]
        public JoinRoomMode ClientJoinRoomMode;
        public MatchmakingMode MatchingType;
        /// <summary>SQL query to filter room matches. For default-typed lobbies, use ExpectedCustomRoomProperties instead.</summary>
        public string SqlLobbyFilter;

        #endregion

        #region Options

        private readonly Photon.Realtime.RoomOptions _roomOptions = new Photon.Realtime.RoomOptions();

        private readonly OpJoinRandomRoomParams _joinRandomRoomParams = new OpJoinRandomRoomParams
        {
            TypedLobby = Photon.Realtime.TypedLobby.Default
        };

        private readonly RaiseEventOptions _raiseToMasterOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.MasterClient
        };

        private readonly RaiseEventOptions _eventOptions = new RaiseEventOptions
        {
            TargetActors = new int[1]
        };

        private readonly EnterRoomParams _enterRoomParams = new EnterRoomParams
        {
            Lobby = Photon.Realtime.TypedLobby.Default
        };


        private EnterRoomParams GetEnterRoomParams(
            Hashtable customRoomProperties = null,
            string[] customRoomPropertiesForLobby = null,
            string[] expectedUsers = null,
            Hashtable playerProperties = null)
        {
            _enterRoomParams.RoomOptions = GetRoomOptions(customRoomProperties, customRoomPropertiesForLobby);
            _enterRoomParams.RoomName = RoomName;
            _enterRoomParams.ExpectedUsers = expectedUsers;
            _enterRoomParams.Lobby.Name = LobbyName;
            _enterRoomParams.Lobby.Type = (Photon.Realtime.LobbyType)LobbyType;
            _enterRoomParams.PlayerProperties = playerProperties;
            return _enterRoomParams;
        }

        private OpJoinRandomRoomParams GetJoinRandomRoomParams(Hashtable customRoomProperties = null, object ticket = null, string[] expectedUsers = null)
        {
            _joinRandomRoomParams.ExpectedCustomRoomProperties = customRoomProperties;
            _joinRandomRoomParams.Ticket = ticket;
            _joinRandomRoomParams.ExpectedUsers = expectedUsers;
            _joinRandomRoomParams.ExpectedMaxPlayers = RoomOptions.MaxPlayers;
            _joinRandomRoomParams.MatchingType = MatchingType;
            _joinRandomRoomParams.TypedLobby.Name = LobbyName;
            _joinRandomRoomParams.TypedLobby.Type = (Photon.Realtime.LobbyType)LobbyType;
            _joinRandomRoomParams.SqlLobbyFilter = SqlLobbyFilter;
            
            return _joinRandomRoomParams;
        }

        private OpJoinRandomRoomParams GetJoinRandomRoomParams(JoinRandomRoomData joinRandomRoomData = null)
        {
            Hashtable roomProperties = joinRandomRoomData?.CustomRoomProperties;
            object ticket = joinRandomRoomData?.Ticket;
            string[] expectedUsers = joinRandomRoomData?.ExpectedUsers;
            return GetJoinRandomRoomParams(roomProperties, ticket, expectedUsers);
        }

        private EnterRoomParams GetEnterRoomParams(JoinRoomData joinRoomData = null)
        {
            string[] expectedUsers = joinRoomData?.ExpectedUsers;
            Hashtable playerProperties = joinRoomData?.PlayerProperties;
            return GetEnterRoomParams(expectedUsers, playerProperties);
        }

        private EnterRoomParams GetEnterRoomParams(CreateRoomData createRoomData = null, Hashtable playerProperties = null)
        {
            return GetEnterRoomParams(createRoomData?.CustomRoomProperties, createRoomData?.CustomRoomPropertiesForLobby,
                createRoomData?.ExpectedUsers, playerProperties);
        }

        private EnterRoomParams GetEnterRoomParams(string[] expectedUsers, Hashtable playerProperties)
        {
            return GetEnterRoomParams(null, null, expectedUsers, playerProperties);
        }

        private Photon.Realtime.RoomOptions GetRoomOptions(Hashtable customRoomProperties = null, string[] customRoomPropertiesForLobby = null)
        {
            _roomOptions.IsVisible = RoomOptions.IsVisible;
            _roomOptions.IsOpen = RoomOptions.IsOpen;
            _roomOptions.MaxPlayers = RoomOptions.MaxPlayers;
            _roomOptions.PlayerTtl = RoomOptions.PlayerTtl;
            _roomOptions.EmptyRoomTtl = RoomOptions.EmptyRoomTtl;
            _roomOptions.CleanupCacheOnLeave = RoomOptions.CleanupCacheOnLeave;
            _roomOptions.CustomRoomProperties = customRoomProperties;
            _roomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobby;
            _roomOptions.Plugins = RoomOptions.Plugins;
            _roomOptions.SuppressRoomEvents = RoomOptions.SuppressRoomEvents;
            _roomOptions.SuppressPlayerInfo = RoomOptions.SuppressPlayerInfo;
            _roomOptions.PublishUserId = RoomOptions.PublishUserId;
            _roomOptions.DeleteNullProperties = RoomOptions.DeleteNullProperties;
            _roomOptions.BroadcastPropsChangeToAll = RoomOptions.BroadcastPropsChangeToAll;
#if SERVERSDK
            _roomOptions.CheckUserOnJoin = RoomOptions.CheckUserOnJoin;
#endif
            return _roomOptions;
        }

        #endregion

        private void Update()
        {
            if (IsClientStopped && IsServerStopped)
            {
                _client.Service();
            }
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        public bool ConnectUsingSettings()
        {
            return _client.ConnectUsingSettings(AppSettings);
        }

        private bool StartQuickConnection(JoinRandomRoomData joinRandomRoomData = null, CreateRoomData createRoomData = null)
        {
            if (!IsServerStopped || !IsClientStopped) return false;
            
            SetClientConnectionState(LocalConnectionState.Starting);
            
            OpJoinRandomRoomParams joinParams = GetJoinRandomRoomParams(joinRandomRoomData);
            EnterRoomParams createParams = GetEnterRoomParams(createRoomData);
            
            if (_client.OpJoinRandomOrCreateRoom(joinParams, createParams))
            {
                InitializeNetwork();
                return true;
            }
            
            SetClientConnectionState(LocalConnectionState.Stopped);
            return false;
        }

        private static SendOptions SelectSendOptions(Channel channel)
        {
            return channel == Channel.Reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable;
        }

        private void InitializeNetwork()
        {
            if (_isInitialized)
            {
                NetworkManager.LogWarning("Already initialized.");
                return;
            }

            _isInitialized = true;
            _client.AddCallbackTarget(this);
        }

        private void DeinitializeNetwork()
        {
            if (!_isInitialized)
            {
                NetworkManager.LogWarning("Not initialized.");
                return;
            }

            _isInitialized = false;
            _client.RemoveCallbackTarget(this);
        }

        public override event Action<ClientReceivedDataArgs> OnClientReceivedData;

        public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

        public override string GetConnectionAddress(int connectionId)
        {
            throw new NotSupportedException(nameof(connectionId));
        }

        public override void SetClientAddress(string address)
        {
            throw new NotSupportedException(nameof(address));
        }

        public override event Action<ClientConnectionStateArgs> OnClientConnectionState;

        public override event Action<ServerConnectionStateArgs> OnServerConnectionState;

        public override event Action<RemoteConnectionStateArgs> OnRemoteConnectionState;

        public override void HandleClientConnectionState(ClientConnectionStateArgs connectionStateArgs)
        {
            OnClientConnectionState?.Invoke(connectionStateArgs);
        }

        public override void HandleServerConnectionState(ServerConnectionStateArgs connectionStateArgs)
        {
            OnServerConnectionState?.Invoke(connectionStateArgs);
        }

        public override void HandleRemoteConnectionState(RemoteConnectionStateArgs connectionStateArgs)
        {
            OnRemoteConnectionState?.Invoke(connectionStateArgs);
        }

        public override LocalConnectionState GetConnectionState(bool server)
        {
            return server ? _serverState : _clientState;
        }

        public override RemoteConnectionState GetConnectionState(int connectionId)
        {
            try
            {
                if (_client.CurrentRoom.Players[connectionId] == null)
                {
                    return RemoteConnectionState.Stopped;
                }
            }
            catch
            {
                return RemoteConnectionState.Stopped;
            }

            return _client.CurrentRoom.Players[connectionId].IsInactive ? RemoteConnectionState.Stopped : RemoteConnectionState.Started;
        }

        public override void SendToServer(byte channelId, ArraySegment<byte> segment)
        {
            if (!IsServerStarted) return;
            if (IsClientStarted)
            {
                SendClientHostToServer(channelId, segment);
                return;
            }

            SendOptions sendOptions = SelectSendOptions((Channel)channelId);
            _client.OpRaiseEvent(channelId, segment, _raiseToMasterOptions, sendOptions);
        }

        public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
        {
            if (IsServerStarted && IsClientStarted && connectionId == _client.LocalPlayer.ActorNumber)
            {
                SendToClientHost(channelId, segment);
            }
            else
            {
                _eventOptions.TargetActors[0] = connectionId;
                SendOptions sendOptions = SelectSendOptions((Channel)channelId);
                _client.OpRaiseEvent(channelId, segment, _eventOptions, sendOptions);
            }
        }

        public override void HandleClientReceivedDataArgs(ClientReceivedDataArgs receivedDataArgs)
        {
            OnClientReceivedData?.Invoke(receivedDataArgs);
        }

        public override void HandleServerReceivedDataArgs(ServerReceivedDataArgs receivedDataArgs)
        {
            OnServerReceivedData?.Invoke(receivedDataArgs);
        }

        public override void IterateIncoming(bool server)
        {
            if (IsServerStarted || IsClientStarted)
            {
                IterateClientHostIncoming(server);
            }

            while (_client.LoadBalancingPeer.DispatchIncomingCommands())
            {
            }
        }

        public override int GetMaximumClients()
        {
            return RoomOptions.MaxPlayers;
        }

        public override void SetMaximumClients(int value)
        {
            RoomOptions.MaxPlayers = (byte)value;
        }

        public override void IterateOutgoing(bool server)
        {
            if (server || IsServerStarted)
            {
                _client.LoadBalancingPeer.SendOutgoingCommands();
            }
        }

        public override bool StartConnection(bool server)
        {
            if (server)
            {
                return StartServer();
            }
            
            return ClientJoinRoomMode switch
            {
                JoinRoomMode.ByName => StartClient(),
                JoinRoomMode.Random => StartClientRandomRoom(),
                _ => throw new ArgumentOutOfRangeException(nameof(ClientJoinRoomMode), ClientJoinRoomMode, null)
            };
        }

        public override bool StopConnection(bool server)
        {
            return server ? StopServer() : StopClient();
        }

        public override bool StopConnection(int connectionId, bool immediately)
        {
            _eventOptions.TargetActors[0] = connectionId;
            return _client.OpRaiseEvent(KICK_CLIENT_CODE, null, _eventOptions, SendOptions.SendReliable);
        }

        public override void Shutdown()
        {
            StopConnection(false);
            StopConnection(true);
        }

        public override int GetMTU(byte channel) => 1200;
    }
}