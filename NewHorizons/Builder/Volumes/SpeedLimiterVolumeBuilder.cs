using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class SpeedLimiterVolumeBuilder
    {
        public static SpeedLimiterVolume Make(GameObject planetGO, Sector sector, SpeedLimiterVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<SpeedLimiterVolume>(planetGO, ref sector, info);

            volume.maxSpeed = info.maxSpeed;
            volume.stoppingDistance = info.stoppingDistance;
            volume.maxEntryAngle = info.maxEntryAngle;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
