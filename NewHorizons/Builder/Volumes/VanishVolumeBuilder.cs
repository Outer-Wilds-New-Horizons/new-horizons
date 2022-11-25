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
            var go = new GameObject(typeof(TVolume).Name);
            go.SetActive(false);

            go.transform.parent = sector?.transform ?? planetGO.transform;

            if (!string.IsNullOrEmpty(info.rename))
            {
                go.name = info.rename;
            }

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = planetGO.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    go.transform.parent = newParent;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                }
            }

            go.transform.position = planetGO.transform.TransformPoint(info.position != null ? (Vector3)info.position : Vector3.zero);
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
