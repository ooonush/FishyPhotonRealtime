using System;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IInRoomCallbacks
    {
        void IInRoomCallbacks.OnPlayerEnteredRoom(Player player)
        {
            Debug.Log("OnPlayerEnteredRoom " + player.ActorNumber);
            if (IsServerStarted)
            {
                HandleRemoteConnectionState(RemoteConnectionState.Started, player.ActorNumber);
            }
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player player)
        {
            Debug.Log("OnPlayerLeftRoom " + player.ActorNumber);
            if (IsServerStarted)
            {
                HandleRemoteConnectionState(RemoteConnectionState.Stopped, player.ActorNumber);
            }
            
            if (player.ActorNumber == _client.LocalPlayer.ActorNumber)
            {
                Debug.Log("OnLeftRoom");
                Shutdown();
            }
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            throw new InvalidOperationException("Transport not support switching master client.");
        }
    }
}