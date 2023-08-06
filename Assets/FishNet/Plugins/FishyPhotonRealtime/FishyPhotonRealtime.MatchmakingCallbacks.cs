using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IMatchmakingCallbacks
    {
        void IMatchmakingCallbacks.OnJoinedRoom()
        {
            //If started server
            if (Client.LocalPlayer.IsMasterClient)
            {
                SetServerConnectionState(LocalConnectionState.Started);
                if (_clientState == LocalConnectionState.Starting) // ClientHost started...
                {
                    int hostClientId = Client.LocalPlayer.ActorNumber;
                    var args = new RemoteConnectionStateArgs(RemoteConnectionState.Started, hostClientId, Index);
                    HandleRemoteConnectionState(args);

                    SetClientConnectionState(LocalConnectionState.Started);
                }
            }
            else
            {
                SetClientConnectionState(LocalConnectionState.Started);
            }
        }

        void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogWarning("Join Random Room Failed: " + message);
            SetClientConnectionState(LocalConnectionState.Stopped);
        }

        void IMatchmakingCallbacks.OnLeftRoom()
        {
            Shutdown();
            if (_clientState != LocalConnectionState.Stopped)
            {
                SetClientConnectionState(LocalConnectionState.Stopped);
            }
            if (_serverState != LocalConnectionState.Stopped)
            {
                SetServerConnectionState(LocalConnectionState.Stopped);
            }
        }

        void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
        {
            NetworkManager.LogWarning(message);
            SetServerConnectionState(LocalConnectionState.Stopped);
        }

        void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
        {
            NetworkManager.LogWarning(message);
            SetClientConnectionState(LocalConnectionState.Stopped);
        }

        void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList) { }

        void IMatchmakingCallbacks.OnCreatedRoom() { }
    }
}