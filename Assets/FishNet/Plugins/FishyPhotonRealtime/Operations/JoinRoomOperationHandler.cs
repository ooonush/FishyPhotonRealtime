using System;
using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    internal sealed class JoinRoomOperationHandler : RealtimeOperationHandler, IMatchmakingCallbacks, IConnectionCallbacks
    {
        public JoinRoomOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            SetException(new InvalidOperationException("Failed to create room."));
        }

        public void OnJoinedRoom()
        {
            Complete();
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning("OnJoinRoomFailed: " + message);
            SetException(new InvalidOperationException("On Join Room Failed."));
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogWarning("OnJoinRandomFailed: " + message);
            SetException(new InvalidOperationException("On Join Random Room Failed."));
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            SetException(new InvalidOperationException("Disconnected from server with cause: " + cause));
        }

        protected override void OnCancel()
        {
            Client.OpLeaveRoom(false);
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            SetException(new InvalidOperationException("On Custom Authentication Failed: " + debugMessage));
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            
        }

        public void OnCreatedRoom()
        {
            
        }

        public void OnLeftRoom()
        {
        }

        public void OnConnected()
        {
        }

        public void OnConnectedToMaster()
        {
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }
    }
}