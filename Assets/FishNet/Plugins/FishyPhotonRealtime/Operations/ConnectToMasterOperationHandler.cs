using System;
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

        public override void OnDisconnected(DisconnectCause cause)
        {
            SetException(new InvalidOperationException($"Disconnected from server with cause: {cause}."));
        }

        public override void OnCustomAuthenticationFailed(string debugMessage)
        {
            SetException(new InvalidOperationException(debugMessage));
        }
    }
}