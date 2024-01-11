using System.Threading;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal class DisconnectOperationHandler : ConnectionOperationHandler
    {
        public DisconnectOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Complete();
        }

        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
            Complete();
        }
    }
}