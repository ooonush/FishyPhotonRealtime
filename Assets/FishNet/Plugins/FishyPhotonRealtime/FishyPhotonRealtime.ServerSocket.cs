using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime
    {
        private LocalConnectionState _serverState;

        private bool StartServer()
        {
            if (_serverState != LocalConnectionState.Stopped) return false;

            SetServerConnectionState(LocalConnectionState.Starting);

            Client.AddCallbackTarget(this);

            bool succeeded = Client.OpCreateRoom(new EnterRoomParams
            {
                RoomOptions = new RoomOptions
                {
                    MaxPlayers = _maxPlayers
                }
            });

            if (!succeeded)
            {
                SetClientConnectionState(LocalConnectionState.Stopping);
                SetClientConnectionState(LocalConnectionState.Stopped);
            }

            return succeeded;
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
            if (_serverState == LocalConnectionState.Stopping || _serverState == LocalConnectionState.Stopped)
            {
                return false;
            }

            if (_clientState == LocalConnectionState.Starting || _clientState == LocalConnectionState.Started)
            {
                StopClientHost();
            }

            SetServerConnectionState(LocalConnectionState.Stopping);
            LeaveRoom();
            Client.RemoveCallbackTarget(this);
            SetServerConnectionState(LocalConnectionState.Stopped);

            return true;
        }
    }
}