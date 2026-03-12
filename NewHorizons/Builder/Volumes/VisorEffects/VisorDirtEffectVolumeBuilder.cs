using NewHorizons.External.Modules.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes.VisorEffects
{
    public static class VisorDirtEffectVolumeBuilder
    {
        public static VisorDirtEffectVolume Make(GameObject planetGO, Sector sector, VisorEffectModule.DirtEffectVolumeInfo info)
        {
            var volume = PriorityVolumeBuilder.Make<VisorDirtEffectVolume>(planetGO, ref sector, info);

            volume._dirtAccumulationRate = info.dirtAccumulationRate;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
