using System.Collections.Generic;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IConnectionCallbacks
    {
        void IConnectionCallbacks.OnConnected()
        {
            
        }

        void IConnectionCallbacks.OnConnectedToMaster()
        {
            
        }

        void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
        {
            Shutdown();
        }

        void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
        {
            
        }

        void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            
        }

        void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
        {
            
        }
    }
}