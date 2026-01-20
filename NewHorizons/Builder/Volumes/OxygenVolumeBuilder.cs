using NewHorizons.External.Modules.Volumes.VolumeInfos;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class OxygenVolumeBuilder
    {
        public static OxygenVolume Make(GameObject planetGO, Sector sector, OxygenVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<OxygenVolume>(planetGO, ref sector, info);

            volume._treeVolume = info.treeVolume;
            volume._playRefillAudio = info.playRefillAudio;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
