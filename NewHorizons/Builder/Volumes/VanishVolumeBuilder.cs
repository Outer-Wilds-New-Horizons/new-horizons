using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VanishVolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VanishVolumeInfo info) where TVolume : VanishVolume
        {
            if (info.shape != null && info.shape?.useShape == false)
            {
                NHLogger.LogError($"Destruction/VanishVolumes only support colliders. Affects planet [{planetGO.name}]. Set useShape to false.");
            }

            // VanishVolume is only compatible with sphere colliders
            // If info.shape was null, it will still default to using a sphere with info.radius, just make sure it does so with a collider
            info.shape ??= new();
            info.shape.useShape = false;

            var volume = VolumeBuilder.Make<TVolume>(planetGO, sector, info);

            var collider = volume.gameObject.GetComponent<Collider>();
            volume._collider = collider;
            volume._shrinkBodies = info.shrinkBodies;
            volume._onlyAffectsPlayerAndShip = info.onlyAffectsPlayerRelatedBodies;

            return volume;
        }
    }
}
