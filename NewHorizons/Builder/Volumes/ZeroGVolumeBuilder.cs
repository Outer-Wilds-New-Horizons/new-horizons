using NewHorizons.External.Modules.Volumes.VolumeInfos;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class ZeroGVolumeBuilder
    {
        public static ZeroGVolume Make(GameObject planetGO, Sector sector, PriorityVolumeInfo info)
        {
            var volume = PriorityVolumeBuilder.Make<ZeroGVolume>(planetGO, ref sector, info);

            volume._inheritable = true;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
