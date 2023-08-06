using ExitGames.Client.Photon;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IInRoomCallbacks
    {
        void IInRoomCallbacks.OnPlayerEnteredRoom(Player player)
        {
            if (NetworkManager.IsServer)
            {
                HandleRemoteConnectionState(RemoteConnectionState.Started, player.ActorNumber);
            }
        }

        void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
        {
            if (NetworkManager.IsServer)
            {
                HandleRemoteConnectionState(RemoteConnectionState.Stopped, otherPlayer.ActorNumber);
            }

            if (Equals(otherPlayer, Client.LocalPlayer))
            {
                Shutdown();
            }
        }

        void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }

        void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }

        void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient) { }
    }
}