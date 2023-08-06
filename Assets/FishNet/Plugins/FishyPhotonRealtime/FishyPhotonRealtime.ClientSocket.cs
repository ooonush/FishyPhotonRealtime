using System;
using System.Collections.Generic;
using FishNet.Utility.Performance;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime
    {
        private LocalConnectionState _clientState;

        private void SetClientConnectionState(LocalConnectionState state)
        {
            _clientState = state;
            HandleClientConnectionState(new ClientConnectionStateArgs(state, Index));
        }

        private bool StartClient()
        {
            if (_clientState != LocalConnectionState.Stopped) return false;

            if (_serverState == LocalConnectionState.Stopping || _serverState == LocalConnectionState.Stopped)
            {
                SetClientConnectionState(LocalConnectionState.Starting);
                Client.AddCallbackTarget(this);
                return Client.OpJoinRandomRoom();
            }

            return StartClientHost();
        }

        private bool StartClientHost()
        {
            if (_clientState != LocalConnectionState.Stopped) return false;

            SetClientConnectionState(LocalConnectionState.Starting);

            if (_serverState != LocalConnectionState.Started) return true;

            HandleRemoteConnectionState(RemoteConnectionState.Started, Client.LocalPlayer.ActorNumber);
            SetClientConnectionState(LocalConnectionState.Started);

            return true;
        }

        private bool StopClient()
        {
            if (_clientState == LocalConnectionState.Stopping || _clientState == LocalConnectionState.Stopped)
            {
                return false;
            }

            if (_serverState == LocalConnectionState.Starting || _serverState == LocalConnectionState.Started)
            {
                return StopClientHost();
            }

            SetClientConnectionState(LocalConnectionState.Stopping);
            LeaveRoom();
            SetClientConnectionState(LocalConnectionState.Stopped);
            Client.RemoveCallbackTarget(this);

            return true;
        }

        private bool StopClientHost()
        {
            if (_serverState == LocalConnectionState.Stopping || _serverState == LocalConnectionState.Stopped)
            {
                return false;
            }

            if (_clientState == LocalConnectionState.Stopping || _clientState == LocalConnectionState.Stopped)
            {
                return false;
            }

            SetClientConnectionState(LocalConnectionState.Stopping);
            HandleRemoteConnectionState(RemoteConnectionState.Stopped, Client.LocalPlayer.ActorNumber);
            DisposeClientHost();
            SetClientConnectionState(LocalConnectionState.Stopped);

            return true;
        }

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
                    int clientId = Client.LocalPlayer.ActorNumber;
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

        private void SendServerToClientHost(int channelId, ArraySegment<byte> payload)
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