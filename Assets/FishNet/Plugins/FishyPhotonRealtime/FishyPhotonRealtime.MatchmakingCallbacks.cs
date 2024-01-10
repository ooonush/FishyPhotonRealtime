using System.Collections.Generic;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IMatchmakingCallbacks
    {
        void IMatchmakingCallbacks.OnLeftRoom()
        {
            Shutdown();
        }

        void IMatchmakingCallbacks.OnCreatedRoom()
        {
        }

        void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
        {
        }

        void IMatchmakingCallbacks.OnJoinedRoom()
        {
        }

        void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
        {
        }

        void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
        {
        }

        void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }
    }
}