using NewHorizons.External.Modules;
using NewHorizons.External.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class SpeedTrapVolumeBuilder
    {
        public static SpeedTrapVolume Make(GameObject planetGO, Sector sector, SpeedTrapVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<SpeedTrapVolume>(planetGO, sector, info);

            volume._speedLimit = info.speedLimit;
            volume._acceleration = info.acceleration;

            return volume;
        }
    }
}
