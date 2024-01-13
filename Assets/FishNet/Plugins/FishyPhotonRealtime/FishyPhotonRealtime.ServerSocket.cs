using System;
using System.Threading.Tasks;
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
            if (StartServer(GetEnterRoomParams(createRoomData), out Task task))
            {
                Run(task);
                return true;
            }

            return false;
        }

        public async Task StartServerAsync(CreateRoomData createRoomData = null)
        {
            if (!StartServer(GetEnterRoomParams(createRoomData), out Task task))
            {
                throw new Exception("Failed to start server.");
            }
            await task;
        }
        
        private bool StartServer(EnterRoomParams enterRoomParams, out Task task)
        {
            task = null;
            if (!IsServerStopped) return false;
            if (_client == null)
            {
                Debug.LogWarning("Client not initialized. Call ConnectAsync() first.");
                return false;
            }
            
            SetServerConnectionState(LocalConnectionState.Starting);
            
            if (_client.OpCreateRoom(enterRoomParams))
            {
                task = HandleServerStartingAsync(new JoinRoomOperationHandler(_client));
                return true;
            }
            
            SetServerConnectionState(LocalConnectionState.Stopped);
            return false;
        }

        private async Task HandleServerStartingAsync(RealtimeOperationHandler operation)
        {
            try
            {
                await operation.Task;
            }
            catch (Exception)
            {
                StopServer();
                throw;
            }
            SetServerConnectionState(LocalConnectionState.Started);
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
            if (_client is { InRoom: true })
            {
                _client.OpLeaveRoom(false);
            }
            SetServerConnectionState(LocalConnectionState.Stopped);

            return true;
        }
    }
}