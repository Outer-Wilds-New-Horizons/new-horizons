using NewHorizons.External.Modules.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes.VisorEffects
{
    public static class VisorRainEffectVolumeBuilder
    {
        public static VisorRainEffectVolume Make(GameObject planetGO, Sector sector, VisorEffectModule.RainEffectVolumeInfo info)
        {
            var volume = PriorityVolumeBuilder.Make<VisorRainEffectVolume>(planetGO, ref sector, info);

            volume._rainDirection = VisorRainEffectVolume.RainDirection.Radial;
            volume._dropletRate = info.dropletRate;
            volume._streakRate = info.streakRate;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
