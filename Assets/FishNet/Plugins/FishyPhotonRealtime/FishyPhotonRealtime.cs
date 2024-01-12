using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : Transport, IConnectionCallbacks
    {
        private const int KICK_CLIENT_CODE = 2;
        /// <summary>AppId for Realtime or PUN.</summary>
        public string AppIdRealtime;
        /// <summary>The AppVersion can be used to identify builds and will split the AppId distinct "Virtual AppIds" (important for matchmaking).</summary>
        public string AppVersion;
        /// <summary>Defines how authentication is done. On each system, once or once via a WSS connection (safe).</summary>
        public AuthModeOption AuthMode = AuthModeOption.Auth;
        /// <summary>The network level protocol to use.</summary>
        public ConnectionProtocol Protocol = ConnectionProtocol.Udp;
        /// <summary>Enables a fallback to another protocol in case a connect to the Name Server fails.</summary>
        /// <remarks>See: LoadBalancingClient.EnableProtocolFallback.</remarks>
        public bool EnableProtocolFallback = true;
        /// <summary>If true, the client will request the list of currently available lobbies.</summary>
        public bool EnableLobbyStatistics;
        /// <summary>The MatchmakingMode affects how rooms get filled. By default, the server fills rooms.</summary>
        public MatchmakingMode MatchmakingMode;
        /// <summary>Set to a previous BestRegionSummary value before connecting.</summary>
        /// <remarks>
        /// This is a value used when the client connects to the "Best Region".<br/>
        /// If this is null or empty, all regions gets pinged. Providing a previous summary on connect,
        /// speeds up best region selection and makes the previously selected region "sticky".<br/>
        ///
        /// Unity clients should store the BestRegionSummary in the PlayerPrefs.
        /// You can store the new result by implementing <see cref="IConnectionCallbacks.OnConnectedToMaster"/>.
        /// If <see cref="LoadBalancingClient.SummaryToCache"/> is not null, store this string.
        /// To avoid storing the value multiple times, you could set SummaryToCache to null.
        /// </remarks>
        [NonSerialized]
        public string BestRegionSummaryFromStorage;
        /// <summary>Can be set to any of the Photon Cloud's region names to directly connect to that region.</summary>
        public string FixedRegion;
        [SerializeField] private TypedLobby _lobby;
        [SerializeField] private RoomOptions _roomOptions;

        public TypedLobby Lobby
        {
            get => _lobby;
            set
            {
                if (_client.InLobby)
                {
                    Debug.LogError("Cannot change lobby while in a lobby.");
                    return;
                }
                _lobby = value;
            }
        }

        /// <summary>The address (hostname or IP) of the server to connect to.</summary>
        [Header("Server")]
        public string Server;
        /// <summary>If not null, this sets the port of the first Photon server to connect to (that will "forward" the client as needed).</summary>
        public int Port;
        /// <summary>The address (hostname or IP and port) of the proxy server.</summary>
        public string ProxyServer;

        private LoadBalancingClient _client;

        public bool InLobby => _client.InLobby;
        public RoomOptions RoomOptions => _roomOptions;
        public Room CurrentRoom => _client?.CurrentRoom;
        public string CloudRegion => _client?.CloudRegion;
        public bool IsConnected { get; private set; }
        public bool IsConnecting { get; private set; }

        #region Options
        private readonly AppSettings _appSettings = new AppSettings();
        private readonly Photon.Realtime.RoomOptions _realtimeRoomOptions = new Photon.Realtime.RoomOptions();

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

        private AppSettings GetAppSettings()
        {
            _appSettings.NetworkLogging = DebugLevel.INFO;
            _appSettings.AppIdRealtime = AppIdRealtime;
            _appSettings.AppVersion = AppVersion;
            _appSettings.FixedRegion = FixedRegion;
            _appSettings.BestRegionSummaryFromStorage = BestRegionSummaryFromStorage;
            _appSettings.Server = Server;
            _appSettings.Port = Port;
            _appSettings.ProxyServer = ProxyServer;
            _appSettings.Protocol = Protocol;
            _appSettings.EnableProtocolFallback = EnableProtocolFallback;
            _appSettings.AuthMode = AuthMode;
            _appSettings.EnableLobbyStatistics = EnableLobbyStatistics;
            
            return _appSettings;
        }

        private EnterRoomParams GetEnterRoomParams(
            string roomName = null,
            Hashtable customRoomProperties = null,
            string[] customRoomPropertiesForLobby = null,
            string[] expectedUsers = null,
            Hashtable playerProperties = null)
        {
            _enterRoomParams.RoomOptions = GetRoomOptions(customRoomProperties, customRoomPropertiesForLobby);
            _enterRoomParams.RoomName = roomName;
            _enterRoomParams.ExpectedUsers = expectedUsers;
            _enterRoomParams.Lobby.Name = Lobby.Name;
            _enterRoomParams.Lobby.Type = (Photon.Realtime.LobbyType)Lobby.Type;
            _enterRoomParams.PlayerProperties = playerProperties;
            return _enterRoomParams;
        }

        private OpJoinRandomRoomParams GetJoinRandomRoomParams(Hashtable customRoomProperties = null,
            object ticket = null, string[] expectedUsers = null, string sqlLobbyFilter = null)
        {
            _joinRandomRoomParams.ExpectedCustomRoomProperties = customRoomProperties;
            _joinRandomRoomParams.Ticket = ticket;
            _joinRandomRoomParams.ExpectedUsers = expectedUsers;
            _joinRandomRoomParams.SqlLobbyFilter = sqlLobbyFilter;
            _joinRandomRoomParams.ExpectedMaxPlayers = _roomOptions.MaxPlayers;
            _joinRandomRoomParams.MatchingType = MatchmakingMode;
            _joinRandomRoomParams.TypedLobby.Name = Lobby.Name;
            _joinRandomRoomParams.TypedLobby.Type = (Photon.Realtime.LobbyType)Lobby.Type;
            
            return _joinRandomRoomParams;
        }

        private OpJoinRandomRoomParams GetJoinRandomRoomParams(JoinRandomRoomData joinRandomRoomData = null)
        {
            Hashtable roomProperties = joinRandomRoomData?.CustomRoomProperties;
            object ticket = joinRandomRoomData?.Ticket;
            string[] expectedUsers = joinRandomRoomData?.ExpectedUsers;
            string sqlLobbyFilter = joinRandomRoomData?.SqlLobbyFilter;
            return GetJoinRandomRoomParams(roomProperties, ticket, expectedUsers, sqlLobbyFilter);
        }

        private EnterRoomParams GetEnterRoomParams(JoinRoomData joinRoomData = null)
        {
            string[] expectedUsers = joinRoomData?.ExpectedUsers;
            Hashtable playerProperties = joinRoomData?.PlayerProperties;
            string roomName = joinRoomData?.RoomName;
            return GetEnterRoomParams(roomName, expectedUsers, playerProperties);
        }

        private EnterRoomParams GetEnterRoomParams(CreateRoomData createRoomData = null, Hashtable playerProperties = null)
        {
            return GetEnterRoomParams(createRoomData?.RoomName, createRoomData?.CustomRoomProperties, createRoomData?.CustomRoomPropertiesForLobby,
                createRoomData?.ExpectedUsers, playerProperties);
        }

        private EnterRoomParams GetEnterRoomParams(string roomName, string[] expectedUsers, Hashtable playerProperties)
        {
            return GetEnterRoomParams(roomName, null, null, expectedUsers, playerProperties);
        }

        private Photon.Realtime.RoomOptions GetRoomOptions(Hashtable customRoomProperties = null, string[] customRoomPropertiesForLobby = null)
        {
            _realtimeRoomOptions.IsVisible = _roomOptions.IsVisible;
            _realtimeRoomOptions.IsOpen = _roomOptions.IsOpen;
            _realtimeRoomOptions.MaxPlayers = _roomOptions.MaxPlayers;
            _realtimeRoomOptions.PlayerTtl = RoomOptions.PlayerTtl;
            _realtimeRoomOptions.EmptyRoomTtl = RoomOptions.EmptyRoomTtl;
            _realtimeRoomOptions.CleanupCacheOnLeave = RoomOptions.CleanupCacheOnLeave;
            _realtimeRoomOptions.CustomRoomProperties = customRoomProperties;
            _realtimeRoomOptions.CustomRoomPropertiesForLobby = customRoomPropertiesForLobby;
            _realtimeRoomOptions.Plugins = _roomOptions.Plugins;
            _realtimeRoomOptions.SuppressRoomEvents = _roomOptions.SuppressRoomEvents;
            _realtimeRoomOptions.SuppressPlayerInfo = _roomOptions.SuppressPlayerInfo;
            _realtimeRoomOptions.PublishUserId = _roomOptions.PublishUserId;
            _realtimeRoomOptions.DeleteNullProperties = _roomOptions.DeleteNullProperties;
            _realtimeRoomOptions.BroadcastPropsChangeToAll = _roomOptions.BroadcastPropsChangeToAll;
#if SERVERSDK
            _roomOptions.CheckUserOnJoin = RoomOptions.CheckUserOnJoin;
#endif
            return _realtimeRoomOptions;
        }

        #endregion

        private void Update()
        {
            if (_client != null && !IsClientStarted && !IsServerStarted)
            {
                _client.Service();
            }
        }

        private void OnDestroy()
        {
            Shutdown();
            if (_client != null)
            {
                DeinitializeLoadBalancingClient();
            }
        }

        private static async void Run(Task task)
        {
            await task;
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                Debug.LogWarning("Already connected.");
                return;
            }
            
            if (IsConnecting)
            {
                throw new InvalidOperationException("Already connecting.");
            }
            
            if (_client != null)
            {
                throw new InvalidOperationException("Already initialized.");
            }
            
            _client = new LoadBalancingClient();
            _client.AddCallbackTarget(this);
            
            if (_client.ConnectUsingSettings(GetAppSettings()))
            {
                IsConnecting = true;
                try
                {
                    var operation = new ConnectToMasterOperationHandler(_client);
                    await operation.Task;
                }
                catch
                {
                    if (_client != null)
                    {
                        DeinitializeLoadBalancingClient();
                    }
                    throw;
                }
                finally
                {
                    IsConnecting = false;
                }
                IsConnected = true;
            }
        }

        public void Disconnect()
        {
            if (_client == null)
            {
                Debug.LogWarning("Not connected.");
                return;
            }
            
            Shutdown();
            DeinitializeLoadBalancingClient();
        }

        public async Task JoinLobbyAsync()
        {
            if (_client.OpJoinLobby(new Photon.Realtime.TypedLobby(_lobby.Name, (Photon.Realtime.LobbyType)_lobby.Type)))
            {
                var operationHandler = new JoinLobbyOperationHandler(_client);
                await operationHandler.Task;
                return;
            }
            
            throw new InvalidOperationException("Unable to join lobby.");
        }

        private void DeinitializeLoadBalancingClient()
        {
            LoadBalancingClient client = _client;
            _client = null;
            IsConnected = false;
            client.RemoveCallbackTarget(this);
            
            if (client.IsConnected)
            {
                client.Disconnect();
                client.Service();
            }
        }

        void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
        {
            DeinitializeLoadBalancingClient();
        }

        void IConnectionCallbacks.OnConnected()
        {
        }

        void IConnectionCallbacks.OnConnectedToMaster()
        {
        }

        void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        private static SendOptions SelectSendOptions(Channel channel)
        {
            return channel == Channel.Reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable;
        }

        public override event Action<ClientReceivedDataArgs> OnClientReceivedData;

        public override event Action<ServerReceivedDataArgs> OnServerReceivedData;

        public override string GetConnectionAddress(int connectionId)
        {
            return string.Empty;
        }

        public override void SetClientAddress(string address)
        {
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
            if (CurrentRoom != null && CurrentRoom.Players.TryGetValue(connectionId, out Player player))
            {
                return player.IsInactive ? RemoteConnectionState.Stopped : RemoteConnectionState.Started;
            }
            return RemoteConnectionState.Stopped;
        }

        public override void SendToServer(byte channelId, ArraySegment<byte> segment)
        {
            if (!IsClientStarted) return;
            
            if (IsServerStarted)
            {
                SendClientHostToServer(channelId, segment);
                return;
            }
            
            SendOptions sendOptions = SelectSendOptions((Channel)channelId);
            _client.OpRaiseEvent(channelId, segment, _raiseToMasterOptions, sendOptions);
        }

        public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
        {
            if (!IsServerStarted) return;
            
            if (IsClientStarted && connectionId == _client.LocalPlayer.ActorNumber)
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
            if (server && !IsServerStarted) return;
            if (!server && !IsClientStarted) return;
            
            if (IsServerStarted && IsClientStarted)
            {
                IterateClientHostIncoming(server);
            }
            
            while (_client.LoadBalancingPeer.DispatchIncomingCommands())
            {
            }
        }

        public override int GetMaximumClients()
        {
            return _roomOptions.MaxPlayers;
        }

        public override void SetMaximumClients(int value)
        {
            _roomOptions.MaxPlayers = (byte)value;
        }

        public override void IterateOutgoing(bool server)
        {
            if (server && !IsServerStarted) return;
            if (!server && !IsClientStarted) return;
            
            _client.LoadBalancingPeer.SendOutgoingCommands();
        }

        public override bool StartConnection(bool server)
        {
            return server ? StartServer() : StartClientRandomRoom();
        }

        public override bool StopConnection(bool server)
        {
            return server ? StopServer() : StopClient();
        }

        public override bool StopConnection(int connectionId, bool immediately)
        {
            if (!IsServerStarted) return false;
            
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