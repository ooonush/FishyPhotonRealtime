using System;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    public partial class FishyPhotonRealtime : IOnEventCallback
    {
        void IOnEventCallback.OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == KICK_CLIENT_CODE)
            {
                StopConnection(false);
                return;
            }
            if (photonEvent.Code != (byte)Channel.Unreliable && photonEvent.Code != (byte)Channel.Reliable)
            {
                return;
            }

            var channel = (Channel)photonEvent.Code;

            var data = (byte[])photonEvent.CustomData;
            // using var byteArraySlice = photonEvent.CustomData as ByteArraySlice;
            // if (byteArraySlice != null)
            {
                var segment = new ArraySegment<byte>(data);

                if (IsServerStarted)
                {
                    var args = new ServerReceivedDataArgs(segment, channel, photonEvent.Sender, Index);
                    HandleServerReceivedDataArgs(args);
                }
                else if (IsClientStarted)
                {
                    var args = new ClientReceivedDataArgs(segment, channel, Index);
                    HandleClientReceivedDataArgs(args);
                }
            }
            // else
            // {
            //     Debug.Log(photonEvent.ToStringFull());
            //     Debug.Log(photonEvent.CustomData.GetType());
            // }
        }
    }
}