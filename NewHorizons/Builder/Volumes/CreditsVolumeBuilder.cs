using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using OWML.Utils;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class CreditsVolumeBuilder
    {
        public static LoadCreditsVolume Make(GameObject planetGO, Sector sector, LoadCreditsVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<LoadCreditsVolume>(planetGO, sector, info);

            volume.gameOver = info.gameOver;
            volume.deathType = info.deathType == null ? null : EnumUtils.Parse(info.deathType.ToString(), DeathType.Default);

            return volume;
        }
    }
}
