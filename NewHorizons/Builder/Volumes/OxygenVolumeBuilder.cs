using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class OxygenVolumeBuilder
    {
        public static OxygenVolume Make(GameObject planetGO, Sector sector, VolumesModule.OxygenVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<OxygenVolume>(planetGO, sector, info);

            volume._treeVolume = info.treeVolume;
            volume._playRefillAudio = info.playRefillAudio;

            return volume;
        }
    }
}
