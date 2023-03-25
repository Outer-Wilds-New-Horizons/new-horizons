using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class ChangeStarSystemVolumeBuilder
    {
        public static WarpVolume Make(GameObject planetGO, Sector sector, ChangeStarSystemVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<WarpVolume>(planetGO, sector, info);

            volume.TargetSolarSystem = info.targetStarSystem;

            return volume;
        }
    }
}
