using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime.FishNet.Plugins.FishyPhotonRealtime
{
    [CreateAssetMenu(menuName = "FishNet/Photon Realtime/Photon Settings Asset", fileName = "Photon Settings")]
    public sealed class PhotonSettingsAsset : ScriptableObject
    {
        [SerializeField] private AppSettings _appSettings;
        public AppSettings AppSettings => _appSettings;
    }
}