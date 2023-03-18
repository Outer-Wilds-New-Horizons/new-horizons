using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External.Modules;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Volumes
{
    public static class VanishVolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumesModule.VanishVolumeInfo info) where TVolume : VanishVolume
        {
            var go = GeneralPropBuilder.MakeNew(typeof(TVolume).Name, planetGO, sector, info);
            go.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var collider = go.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = info.radius;

            var owCollider = go.AddComponent<OWCollider>();
            owCollider._collider = collider;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._owCollider = owCollider;

            var volume = go.AddComponent<TVolume>();

            volume._collider = collider;
            volume._shrinkBodies = info.shrinkBodies;
            volume._onlyAffectsPlayerAndShip = info.onlyAffectsPlayerAndShip;

            go.SetActive(true);

            return volume;
        }
    }
}
