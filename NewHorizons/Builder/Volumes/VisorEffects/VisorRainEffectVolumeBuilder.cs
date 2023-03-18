using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VisorRainEffectVolumeBuilder
    {
        public static VisorRainEffectVolume Make(GameObject planetGO, Sector sector, VolumesModule.VisorEffectModule.RainEffectVolumeInfo info)
        {
            var volume = PriorityVolumeBuilder.Make<VisorRainEffectVolume>(planetGO, sector, info);

            volume._rainDirection = VisorRainEffectVolume.RainDirection.Radial;
            volume._dropletRate = info.dropletRate;
            volume._streakRate = info.streakRate;

            return volume;
        }
    }
}
