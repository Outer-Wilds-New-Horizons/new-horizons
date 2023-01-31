using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class SpeedTrapVolumeBuilder
    {
        public static SpeedTrapVolume Make(GameObject planetGO, Sector sector, VolumesModule.SpeedTrapVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<SpeedTrapVolume>(planetGO, sector, info);

            volume._speedLimit = info.speedLimit;
            volume._acceleration = info.acceleration;

            return volume;
        }
    }
}
