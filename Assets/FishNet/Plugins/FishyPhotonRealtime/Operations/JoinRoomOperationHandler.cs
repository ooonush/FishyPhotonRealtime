using System.Threading;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal sealed class JoinRoomOperationHandler : MatchmakingOperationHandler
    {
        public JoinRoomOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        public override void OnJoinedRoom()
        {
            Complete();
        }
    }
}