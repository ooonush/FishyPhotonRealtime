using System;
using System.Collections.Generic;
using System.Threading;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal class JoinLobbyOperationHandler : RealtimeOperationHandler, ILobbyCallbacks, IConnectionCallbacks
    {
        public JoinLobbyOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        void ILobbyCallbacks.OnJoinedLobby()
        {
            Complete();
        }

        void ILobbyCallbacks.OnLeftLobby()
        {
            
        }

        void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
        {
        }

        void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
        }

        void IConnectionCallbacks.OnConnected()
        {
        }

        void IConnectionCallbacks.OnConnectedToMaster()
        {
        }

        void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
        {
            SetException(new InvalidOperationException($"Client disconnected due to {cause}."));
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