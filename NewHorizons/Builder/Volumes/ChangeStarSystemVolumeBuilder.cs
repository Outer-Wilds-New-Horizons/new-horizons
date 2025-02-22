using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class ChangeStarSystemVolumeBuilder
    {
        public static WarpVolume Make(GameObject planetGO, Sector sector, ChangeStarSystemVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<WarpVolume>(planetGO, sector, info);

            volume.TargetSolarSystem = info.targetStarSystem;
            volume.TargetSpawnID = info.spawnPointID;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
