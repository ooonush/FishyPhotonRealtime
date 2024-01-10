using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IMatchmakingCallbacks
    {
        void IMatchmakingCallbacks.OnCreatedRoom()
        {
            foreach (DictionaryEntry property in _client.CurrentRoom.CustomProperties)
            {
                Debug.Log(property.Value);
            }
        }

        void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
        {
            NetworkManager.LogWarning($"CreateRoom failed. ReturnCode: {returnCode} Message: {message}");
            StopConnection(true);
        }

        void IMatchmakingCallbacks.OnJoinedRoom()
        {
            if (_client.LocalPlayer.IsMasterClient && !IsServerStarted)
            {
                SetServerConnectionState(LocalConnectionState.Started);
            }
            if (IsClientStarting)
            {
                SetClientConnectionState(LocalConnectionState.Started);
            }
        }

        void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
        {
            NetworkManager.LogWarning($"JoinRoom failed. ReturnCode: {returnCode} Message: {message}");
            StopConnection(false);
        }

        void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
        {
            NetworkManager.LogWarning($"JoinRandomRoom failed. ReturnCode: {returnCode} Message: {message}");
            StopConnection(false);
        }

        void IMatchmakingCallbacks.OnLeftRoom()
        {
            Debug.Log("OnLeftRoom");
            StopConnection(false);
        }

        void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }
    }
}