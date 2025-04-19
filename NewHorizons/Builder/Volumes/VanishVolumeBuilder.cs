using NewHorizons.External.Modules.Volumes.VolumeInfos;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VanishVolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VanishVolumeInfo info) where TVolume : VanishVolume
        {
            var volume = VolumeBuilder.Make<TVolume>(planetGO, sector, info);

            var collider = volume.gameObject.GetComponent<SphereCollider>();
            volume._collider = collider;
            volume._shrinkBodies = info.shrinkBodies;
            volume._onlyAffectsPlayerAndShip = info.onlyAffectsPlayerRelatedBodies;

            return volume;
        }
    }
}
