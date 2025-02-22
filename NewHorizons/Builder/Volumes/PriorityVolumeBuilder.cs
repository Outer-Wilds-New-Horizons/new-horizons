using NewHorizons.External.Modules.Volumes.VolumeInfos;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class PriorityVolumeBuilder
    {
        public static TVolume MakeExisting<TVolume>(GameObject go, GameObject planetGO, Sector sector, PriorityVolumeInfo info) where TVolume : PriorityVolume
        {
            var volume = VolumeBuilder.MakeExisting<TVolume>(go, planetGO, sector, info);

            volume._layer = info.layer;
            volume.SetPriority(info.priority);

            return volume;
        }


        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, PriorityVolumeInfo info) where TVolume : PriorityVolume
        {
            var volume = VolumeBuilder.Make<TVolume>(planetGO, sector, info);

            volume._layer = info.layer;
            volume.SetPriority(info.priority);

            return volume;
        }
    }
}
