using NewHorizons.External.Modules.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes.VisorEffects
{
    public static class VisorFrostEffectVolumeBuilder
    {
        public static VisorFrostEffectVolume Make(GameObject planetGO, Sector sector, VisorEffectModule.FrostEffectVolumeInfo info)
        {
            var volume = PriorityVolumeBuilder.Make<VisorFrostEffectVolume>(planetGO, sector, info);

            volume._frostRate = info.frostRate;
            volume._maxFrost = info.maxFrost;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
