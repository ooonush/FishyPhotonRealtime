using System;
using System.Collections.Generic;
using FishNet.Utility.Performance;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime
    {
        private readonly struct ClientHostSendData
        {
            public byte[] Data { get; }
            public int Length { get; }
            public Channel Channel { get; }

            public ClientHostSendData(Channel channel, ArraySegment<byte> data)
            {
                if (data.Array == null) throw new InvalidOperationException();

                Data = new byte[data.Count];
                Length = data.Count;
                Buffer.BlockCopy(data.Array, data.Offset, Data, 0, Length);
                Channel = channel;
            }
        }

        private Queue<ClientHostSendData> _clientHostSendQueue;
        private Queue<ClientHostSendData> _clientHostReceiveQueue;

        private LocalConnectionState _clientState;

        private bool IsClientStarting => _clientState == LocalConnectionState.Starting;
        private bool IsClientStarted => _clientState == LocalConnectionState.Started;
        private bool IsClientStopping => _clientState == LocalConnectionState.Stopping;
        private bool IsClientStopped => _clientState == LocalConnectionState.Stopped;

        private void SetClientConnectionState(LocalConnectionState state)
        {
            if (_clientState == state) return;
            
            if (state == LocalConnectionState.Stopped && IsClientStopping)
            {
                SetClientConnectionState(LocalConnectionState.Stopping);
            }
            
            _clientState = state;
            HandleClientConnectionState(new ClientConnectionStateArgs(state, Index));
        }

        private bool StopClient()
        {
            if (IsClientStopping || IsClientStopped)
            {
                return false;
            }

            if (IsServerStarting || IsServerStarted)
            {
                return StopClientHost();
            }

            SetClientConnectionState(LocalConnectionState.Stopping);

            if (_client.InRoom)
            {
                _client.OpLeaveRoom(false);
            }
            DeinitializeNetwork();

            SetClientConnectionState(LocalConnectionState.Stopped);

            return true;
        }

        private bool CanStartClient => IsClientStopped && !IsServerStopping;

        public bool StartClient(JoinRoomData joinRoomData = null)
        {
            return StartClient(GetEnterRoomParams(joinRoomData));
        }

        public bool StartClientRandomRoom(JoinRandomRoomData joinRandomRoomData = null)
        {
            return StartClientRandomRoom(GetJoinRandomRoomParams(joinRandomRoomData));
        }

        private bool StartClientHost()
        {
            if (!IsServerStarted) return IsServerStarting;
            HandleRemoteConnectionState(RemoteConnectionState.Started, _client.LocalPlayer.ActorNumber);
            SetClientConnectionState(LocalConnectionState.Started);
            return true;
        }

        private bool StartClient(EnterRoomParams enterRoomParams)
        {
            if (!CanStartClient) return false;

            SetClientConnectionState(LocalConnectionState.Starting);

            if (IsServerStarted || IsServerStarting)
            {
                return StartClientHost();
            }

            if (_client.OpJoinRoom(enterRoomParams))
            {
                InitializeNetwork();
                return true;
            }

            SetClientConnectionState(LocalConnectionState.Stopped);
            return false;
        }

        private bool StartClientRandomRoom(OpJoinRandomRoomParams joinRandomRoomParams)
        {
            if (!CanStartClient) return false;

            SetClientConnectionState(LocalConnectionState.Starting);

            if (IsServerStarted || IsServerStarting)
            {
                return StartClientHost();
            }

            if (_client.OpJoinRandomRoom(joinRandomRoomParams))
            {
                InitializeNetwork();
                return true;
            }

            SetClientConnectionState(LocalConnectionState.Stopped);
            return false;
        }

        private bool StopClientHost()
        {
            if (IsServerStopping || IsServerStopped)
            {
                return false;
            }

            if (IsClientStopping || IsClientStopped)
            {
                return false;
            }

            SetClientConnectionState(LocalConnectionState.Stopping);
            HandleRemoteConnectionState(RemoteConnectionState.Stopped, _client.LocalPlayer.ActorNumber);
            DisposeClientHost();
            SetClientConnectionState(LocalConnectionState.Stopped);

            return true;
        }

        private void DisposeClientHost()
        {
            _clientHostSendQueue?.Clear();
            _clientHostReceiveQueue?.Clear();

            _clientHostSendQueue = null;
            _clientHostReceiveQueue = null;
        }

        private void IterateClientHostIncoming(bool asServer)
        {
            if (asServer)
            {
                while (_clientHostSendQueue != null && _clientHostSendQueue.Count > 0)
                {
                    ClientHostSendData packet = _clientHostSendQueue.Dequeue();
                    var segment = new ArraySegment<byte>(packet.Data, 0, packet.Length);
                    int clientId = _client.LocalPlayer.ActorNumber;
                    HandleServerReceivedDataArgs(new ServerReceivedDataArgs(segment, packet.Channel, clientId,
                        Index));
                }
            }
            else
            {
                while (_clientHostReceiveQueue != null && _clientHostReceiveQueue.Count > 0)
                {
                    ClientHostSendData packet = _clientHostReceiveQueue.Dequeue();
                    var segment = new ArraySegment<byte>(packet.Data, 0, packet.Length);
                    HandleClientReceivedDataArgs(new ClientReceivedDataArgs(segment, packet.Channel, Index));
                    ByteArrayPool.Store(packet.Data);
                }
            }
        }

        private void SendToClientHost(int channelId, ArraySegment<byte> payload)
        {
            _clientHostReceiveQueue ??= new Queue<ClientHostSendData>();

            _clientHostReceiveQueue.Enqueue(new ClientHostSendData((Channel)channelId, payload));
        }

        private void SendClientHostToServer(int channelId, ArraySegment<byte> payload)
        {
            _clientHostSendQueue ??= new Queue<ClientHostSendData>();

            _clientHostSendQueue.Enqueue(new ClientHostSendData((Channel)channelId, payload));
        }
    }
}