using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules;
using NewHorizons.External.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class CreditsVolumeBuilder
    {
        public static LoadCreditsVolume Make(GameObject planetGO, Sector sector, LoadCreditsVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<LoadCreditsVolume>(planetGO, sector, info);

            volume.creditsType = info.creditsType;

            return volume;
        }
    }
}
