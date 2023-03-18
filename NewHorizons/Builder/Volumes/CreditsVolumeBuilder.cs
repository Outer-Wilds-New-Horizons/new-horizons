using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class CreditsVolumeBuilder
    {
        public static LoadCreditsVolume Make(GameObject planetGO, Sector sector, VolumesModule.LoadCreditsVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<LoadCreditsVolume>(planetGO, sector, info);

            volume.creditsType = info.creditsType;

            return volume;
        }
    }
}
