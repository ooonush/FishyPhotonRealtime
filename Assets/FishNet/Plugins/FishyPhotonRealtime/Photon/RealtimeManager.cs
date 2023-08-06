using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Realtime
{
    public class PhotonRealtimeManager : MonoBehaviour, IConnectionCallbacks
    {
        [SerializeField] private AppSettings _appSettings;

        public readonly LoadBalancingClient Client = new LoadBalancingClient();

        private void Awake()
        {
            Debug.Log(Client.Server.ToString());
            Client.AddCallbackTarget(this);
            Client.StateChanged += OnStateChanged;
            Client.AppId = _appSettings.AppIdRealtime;
            Client.ConnectToNameServer();
        }

        private void Update()
        {
            if (!Client.InRoom)
            {
                Client.Service();
            }
        }

        private void OnDestroy()
        {
            Client.Disconnect();
        }

        private void OnStateChanged(ClientState prev, ClientState next)
        {
        }

        public void OnConnected()
        {
        }

        public void OnConnectedToMaster()
        {
        }

        public void OnDisconnected(DisconnectCause cause)
        {
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            _appSettings.FixedRegion = regionHandler.BestRegion.Code;
            Client.ConnectToRegionMaster(regionHandler.BestRegion.Code);
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
        }
    }
}