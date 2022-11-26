using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VisorFrostEffectVolumeBuilder
    {
        public static VisorFrostEffectVolume Make(GameObject planetGO, Sector sector, VolumesModule.VisorEffectModule.FrostEffectVolumeInfo info)
        {
            var volume = PriorityVolumeBuilder.Make<VisorFrostEffectVolume>(planetGO, sector, info);

            volume._frostRate = info.frostRate;
            volume._maxFrost = info.maxFrost;

            return volume;
        }
    }
}
