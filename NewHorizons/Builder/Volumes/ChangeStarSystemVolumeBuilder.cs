using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class ChangeStarSystemVolumeBuilder
    {
        public static WarpVolume Make(GameObject planetGO, Sector sector, VolumesModule.ChangeStarSystemVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<WarpVolume>(planetGO, sector, info);

            volume.TargetSolarSystem = info.targetStarSystem;

            return volume;
        }
    }
}
