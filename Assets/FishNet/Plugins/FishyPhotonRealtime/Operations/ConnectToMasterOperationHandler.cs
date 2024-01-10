using System.Threading;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal sealed class ConnectToMasterOperationHandler : ConnectionOperationHandler
    {
        public ConnectToMasterOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        public override void OnConnectedToMaster()
        {
            Complete();
        }
    }
}