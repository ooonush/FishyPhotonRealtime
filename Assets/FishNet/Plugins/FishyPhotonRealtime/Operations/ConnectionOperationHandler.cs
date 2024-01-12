using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal abstract class ConnectionOperationHandler : RealtimeOperationHandler, IConnectionCallbacks
    {
        protected ConnectionOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        public virtual void OnConnected()
        {
        }

        public virtual void OnConnectedToMaster()
        {
        }

        public virtual void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public virtual void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public virtual void OnDisconnected(DisconnectCause cause)
        {
        }

        public virtual void OnCustomAuthenticationFailed(string debugMessage)
        {
        }
    }
}