using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class PriorityVolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumesModule.PriorityVolumeInfo info) where TVolume : PriorityVolume
        {
            var volume = VolumeBuilder.Make<TVolume>(planetGO, sector, info);

            volume.SetPriority(info.priority);

            return volume;
        }
    }
}
