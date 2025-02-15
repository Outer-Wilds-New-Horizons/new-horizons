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

            volume.creditsType = info.creditsType;
            volume.gameOverText = info.gameOver?.text;
            volume.deathType = info.deathType == null ? null : EnumUtils.Parse(info.deathType.ToString(), DeathType.Default);
            volume.colour = info.gameOver?.colour?.ToColor();
            volume.condition = info.gameOver?.condition;

            return volume;
        }
    }
}
