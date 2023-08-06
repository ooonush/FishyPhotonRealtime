using UnityEngine;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : Transport
    {
        [SerializeField] private byte _maxPlayers = 20;
        [SerializeField] private PhotonRealtimeManager _realtimeManager;

        private const int KICK_CLIENT_CODE = 2;
        private LoadBalancingClient Client => _realtimeManager.Client;

        private readonly RaiseEventOptions _raiseToMasterOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.MasterClient
        };

        private readonly RaiseEventOptions _eventOptions = new RaiseEventOptions
        {
            TargetActors = new int[1]
        };

        private bool LeaveRoom()
        {
            return Client.InRoom && Client.OpLeaveRoom(false);
        }

        private void OnDestroy() => Shutdown();

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
                if (Client.CurrentRoom.Players[connectionId] == null)
                {
                    return RemoteConnectionState.Stopped;
                }
            }
            catch
            {
                return RemoteConnectionState.Stopped;
            }

            return Client.CurrentRoom.Players[connectionId].IsInactive ? RemoteConnectionState.Stopped : RemoteConnectionState.Started;
        }

        public override void SendToServer(byte channelId, ArraySegment<byte> segment)
        {
            if (_serverState == LocalConnectionState.Started && _clientState == LocalConnectionState.Started)
            {
                SendClientHostToServer(channelId, segment);
                return;
            }

            SendOptions sendOptions = SelectSendOptions((Channel)channelId);
            Client.OpRaiseEvent(channelId, segment, _raiseToMasterOptions, sendOptions);
        }

        public override void SendToClient(byte channelId, ArraySegment<byte> segment, int connectionId)
        {
            if (_serverState == LocalConnectionState.Started && _clientState == LocalConnectionState.Started && connectionId == Client.LocalPlayer.ActorNumber)
            {
                SendServerToClientHost(channelId, segment);
            }
            else
            {
                _eventOptions.TargetActors[0] = connectionId;
                SendOptions sendOptions = SelectSendOptions((Channel)channelId);
                Client.OpRaiseEvent(channelId, segment, _eventOptions, sendOptions);
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
            if (_serverState == LocalConnectionState.Started || _clientState == LocalConnectionState.Started)
            {
                IterateClientHostIncoming(server);
            }

            while (Client.LoadBalancingPeer.DispatchIncomingCommands())
            {
            }
        }

        public override int GetMaximumClients()
        {
            return _maxPlayers;
        }

        public override void SetMaximumClients(int value)
        {
            _maxPlayers = (byte)value;
        }
        
        public override void IterateOutgoing(bool server)
        {
            if (server || _serverState != LocalConnectionState.Started)
            {
                Client.LoadBalancingPeer.SendOutgoingCommands();
            }
        }

        public override bool StartConnection(bool server)
        {
            return server ? StartServer() : StartClient();
        }

        public override bool StopConnection(bool server)
        {
            return server ? StopServer() : StopClient();
        }

        public override bool StopConnection(int connectionId, bool immediately)
        {
            _eventOptions.TargetActors[0] = connectionId;
            return Client.OpRaiseEvent(KICK_CLIENT_CODE, null, _eventOptions, SendOptions.SendReliable);
        }

        public override void Shutdown()
        {
            StopConnection(false);
            StopConnection(true);
        }

        public override int GetMTU(byte channel) => 1200;
    }
}