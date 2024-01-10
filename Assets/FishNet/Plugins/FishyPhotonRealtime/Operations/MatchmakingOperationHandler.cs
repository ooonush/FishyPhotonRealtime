using System;
using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal abstract class MatchmakingOperationHandler : RealtimeOperationHandler, IMatchmakingCallbacks
    {
        protected MatchmakingOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        public virtual void OnCreateRoomFailed(short returnCode, string message)
        {
            SetException(new InvalidOperationException($"Failed to create room. Return code: {returnCode}. Message: {message}."));
        }

        public virtual void OnJoinRoomFailed(short returnCode, string message)
        {
            SetException(new InvalidOperationException($"Failed to join room. Return code: {returnCode}. Message: {message}."));
        }

        public virtual void OnJoinRandomFailed(short returnCode, string message)
        {
            SetException(new InvalidOperationException($"Failed to join random room. Return code: {returnCode}. Message: {message}."));
        }

        public virtual void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public virtual void OnCreatedRoom()
        {
        }

        public virtual void OnJoinedRoom()
        {
        }

        public virtual void OnLeftRoom()
        {
        }
    }
}