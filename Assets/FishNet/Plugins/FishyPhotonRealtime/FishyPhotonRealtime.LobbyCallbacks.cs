using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal sealed class UpdateRoomListOperationHandler : RealtimeOperationHandler<List<RoomInfo>>, ILobbyCallbacks, IConnectionCallbacks
    {
        public UpdateRoomListOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        void ILobbyCallbacks.OnJoinedLobby()
        {
        }

        void ILobbyCallbacks.OnLeftLobby()
        {
            SetException(new InvalidOperationException("Left lobby when updating room list."));
        }

        void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Complete(roomList);
        }

        void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
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
    
    public partial class FishyPhotonRealtime : ILobbyCallbacks
    {
        public Task<List<RoomInfo>> UpdateRoomListAsync()
        {
            var operation = new UpdateRoomListOperationHandler(_client);
            return operation.Task;
        }

        void ILobbyCallbacks.OnJoinedLobby()
        {
            
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
    }
}