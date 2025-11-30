using NewHorizons.External.Modules.Volumes.VolumeInfos;
using System;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class PriorityVolumeBuilder
    {
        #region obsolete
        // Changed to ref sector
        [Obsolete]
        public static TVolume MakeExisting<TVolume>(GameObject go, GameObject planetGO, Sector sector, PriorityVolumeInfo info) where TVolume : PriorityVolume
            => MakeExisting<TVolume>(go, planetGO, ref sector, info);
        [Obsolete]
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, PriorityVolumeInfo info) where TVolume : PriorityVolume
            => Make<TVolume>(planetGO, ref sector, info);
        #endregion

        public static TVolume MakeExisting<TVolume>(GameObject go, GameObject planetGO, ref Sector sector, PriorityVolumeInfo info) where TVolume : PriorityVolume
        {
            var volume = VolumeBuilder.MakeExisting<TVolume>(go, planetGO, ref sector, info);

            volume._layer = info.layer;
            volume.SetPriority(info.priority);

            return volume;
        }


        public static TVolume Make<TVolume>(GameObject planetGO, ref Sector sector, PriorityVolumeInfo info) where TVolume : PriorityVolume
        {
            var volume = VolumeBuilder.Make<TVolume>(planetGO, ref sector, info);

            volume._layer = info.layer;
            volume.SetPriority(info.priority);

            return volume;
        }
    }
}
