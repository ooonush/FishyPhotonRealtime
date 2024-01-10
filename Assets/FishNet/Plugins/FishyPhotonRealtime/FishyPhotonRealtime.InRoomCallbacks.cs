using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IInRoomCallbacks
    {
        void IInRoomCallbacks.OnPlayerEnteredRoom(Player player)
        {
            if (IsServerStarted)
            {
                HandleRemoteConnectionState(RemoteConnectionState.Started, player.ActorNumber);
            }
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player player)
        {
            if (IsServerStarted)
            {
                HandleRemoteConnectionState(RemoteConnectionState.Stopped, player.ActorNumber);
            }
            
            if (player.ActorNumber == _client.LocalPlayer.ActorNumber)
            {
                Shutdown();
            }
        }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.LogError("Transport not support switching master client.");
            Shutdown();
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }
    }
}