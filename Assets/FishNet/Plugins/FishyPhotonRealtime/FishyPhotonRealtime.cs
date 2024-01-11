using UnityEngine;
using System;
using System.Threading.Tasks;
using Photon.Realtime;
using ExitGames.Client.Photon;
using FishNet.Transporting.PhotonRealtime.FishNet.Plugins.FishyPhotonRealtime;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : Transport
    {
        private const int KICK_CLIENT_CODE = 2;

        [SerializeField] public PhotonSettingsAsset PhotonSettings;
        [SerializeField] public RoomOptions RoomOptions;
        [SerializeField] private ConnectionData _connectionData;
        private readonly LoadBalancingClient _client = new LoadBalancingClient();
        private Task _connectTask;

        private AppSettings AppSettings => PhotonSettings.AppSettings;
        public Room CurrentRoom => _client.CurrentRoom;
        public ConnectionData ConnectionData => _connectionData;
        public bool IsConnected => _client.IsConnected;
        public ClientState State => _client.State;
        public string CloudRegion => _client.CloudRegion;

        #region Options

        private readonly Photon.Realtime.RoomOptions _roomOptions = new Photon.Realtime.RoomOptions();


        private readonly OpJoinRandomRoomParams _joinRandomRoomParams = new OpJoinRandomRoomParams
        {
            TypedLobby = TypedLobby.Default
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
            _enterRoomParams.RoomName = ConnectionData.RoomName;
            _enterRoomParams.ExpectedUsers = expectedUsers;
            _enterRoomParams.Lobby.Name = ConnectionData.LobbyName;
            _enterRoomParams.Lobby.Type = (Photon.Realtime.LobbyType)ConnectionData.LobbyType;
            _enterRoomParams.PlayerProperties = playerProperties;
            return _enterRoomParams;
        }

        private OpJoinRandomRoomParams GetJoinRandomRoomParams(Hashtable customRoomProperties = null, object ticket = null, string[] expectedUsers = null)
        {
            _joinRandomRoomParams.ExpectedCustomRoomProperties = customRoomProperties;
            _joinRandomRoomParams.Ticket = ticket;
            _joinRandomRoomParams.ExpectedUsers = expectedUsers;
            _joinRandomRoomParams.ExpectedMaxPlayers = RoomOptions.MaxPlayers;
            _joinRandomRoomParams.MatchingType = ConnectionData.MatchmakingMode;
            _joinRandomRoomParams.TypedLobby.Name = ConnectionData.LobbyName;
            _joinRandomRoomParams.TypedLobby.Type = (Photon.Realtime.LobbyType)ConnectionData.LobbyType;
            _joinRandomRoomParams.SqlLobbyFilter = ConnectionData.MatchmakingSqlLobbyFilter;
            
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

        private void Awake()
        {
            _client.AddCallbackTarget(this);
        }

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
            _client.RemoveCallbackTarget(this);
        }

        private static async void Run(Task task)
        {
            await task;
        }

        public async Task ConnectAsync()
        {
            if (ConnectUsingSettingsAsync(out Task task))
            {
                await task;
            }
            else
            {
                throw new Exception("Failed to connect using settings.");
            }
        }

        private bool ConnectUsingSettingsAsync(out Task task)
        {
            if (_client.IsConnected)
            {
                task = Task.FromException(new InvalidOperationException("Client still connected"));
                return false;
            }
            if (_client.ConnectUsingSettings(AppSettings))
            {
                var operation = new ConnectToMasterOperationHandler(_client);
                task = operation.Task;
                _connectTask = operation.Task;
                return true;
            }
            
            task = null;
            return false;
        }

        public async Task DisconnectAsync()
        {
            if (_connectTask is { IsCompleted: false })
            {
                await _connectTask;
            }
            if (!_client.IsConnected)
            {
                return;
            }
            var operation = new DisconnectOperationHandler(_client);
            _client.Disconnect();
            await operation.Task;
        }

        private bool StartQuickConnection(JoinRandomRoomData joinRandomRoomData = null, CreateRoomData createRoomData = null)
        {
            if (!IsServerStopped || !IsClientStopped) return false;
            
            SetClientConnectionState(LocalConnectionState.Starting);
            
            OpJoinRandomRoomParams joinParams = GetJoinRandomRoomParams(joinRandomRoomData);
            EnterRoomParams createParams = GetEnterRoomParams(createRoomData);
            
            if (_client.OpJoinRandomOrCreateRoom(joinParams, createParams))
            {
                return true;
            }
            
            SetClientConnectionState(LocalConnectionState.Stopped);
            return false;
        }

        private static SendOptions SelectSendOptions(Channel channel)
        {
            return channel == Channel.Reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable;
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

            return string.IsNullOrEmpty(ConnectionData.RoomName) ? StartClientRandomRoom() : StartClient();
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