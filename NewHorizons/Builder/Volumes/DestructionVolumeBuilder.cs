using NewHorizons.External.Modules;
using NewHorizons.External.Volumes;
using OWML.Utils;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class DestructionVolumeBuilder
    {
        public static DestructionVolume Make(GameObject planetGO, Sector sector, DestructionVolumeInfo info)
        {
            var volume = VanishVolumeBuilder.Make<DestructionVolume>(planetGO, sector, info);

            volume._deathType = EnumUtils.Parse<DeathType>(info.deathType.ToString(), DeathType.Default);

            return volume;
        }
    }
}
