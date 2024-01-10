using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime
    {
        private LocalConnectionState _serverState;

        private bool IsServerStarting => _serverState == LocalConnectionState.Starting;
        private bool IsServerStarted => _serverState == LocalConnectionState.Started;
        private bool IsServerStopping => _serverState == LocalConnectionState.Stopping;
        private bool IsServerStopped => _serverState == LocalConnectionState.Stopped;

        public bool StartServer(CreateRoomData createRoomData = null)
        {
            EnterRoomParams enterRoomParams = GetEnterRoomParams(createRoomData);
            return StartServer(enterRoomParams);
        }

        private bool StartServer(EnterRoomParams enterRoomParams)
        {
            if (!IsServerStopped) return false;

            SetServerConnectionState(LocalConnectionState.Starting);

            if (_client.OpCreateRoom(enterRoomParams))
            {
                Debug.Log(Time.time);
                InitializeNetwork();
                return true;
            }
            
            SetServerConnectionState(LocalConnectionState.Stopped);
            return false;
        }

        private void SetServerConnectionState(LocalConnectionState state)
        {
            _serverState = state;
            HandleServerConnectionState(new ServerConnectionStateArgs(state, Index));
        }

        private void HandleRemoteConnectionState(RemoteConnectionState state, int clientId)
        {
            var args = new RemoteConnectionStateArgs(state, clientId, Index);
            HandleRemoteConnectionState(args);
        }

        private bool StopServer()
        {
            if (IsServerStopping || IsServerStopped)
            {
                return false;
            }

            if (IsClientStarting || IsClientStarted)
            {
                StopClientHost();
            }

            SetServerConnectionState(LocalConnectionState.Stopping);
            if (_client.InRoom)
            {
                _client.OpLeaveRoom(false);
            }
            DeinitializeNetwork();
            SetServerConnectionState(LocalConnectionState.Stopped);

            return true;
        }
    }
}