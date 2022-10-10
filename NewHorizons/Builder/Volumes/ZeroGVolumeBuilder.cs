using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class ZeroGVolumeBuilder
    {
        public static ZeroGVolume Make(GameObject planetGO, Sector sector, VolumesModule.PriorityVolumeInfo info)
        {
            var volume = PriorityVolumeBuilder.Make<ZeroGVolume>(planetGO, sector, info);

            volume._inheritable = true;

            return volume;
        }
    }
}
