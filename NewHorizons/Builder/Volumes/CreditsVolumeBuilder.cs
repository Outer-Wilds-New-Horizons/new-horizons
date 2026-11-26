using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using OWML.Common;
using OWML.Utils;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class CreditsVolumeBuilder
    {
        public static LoadCreditsVolume Make(GameObject planetGO, Sector sector, LoadCreditsVolumeInfo info, IModBehaviour mod)
        {
            var volume = VolumeBuilder.Make<LoadCreditsVolume>(planetGO, ref sector, info);

            volume.gameOver = info.gameOver;
            volume.deathType = info.deathType == null ? null : EnumUtils.Parse(info.deathType.ToString(), DeathType.Default);
            volume.mod = mod;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
