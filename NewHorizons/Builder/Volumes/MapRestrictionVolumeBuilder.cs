using NewHorizons.Components;
using NewHorizons.External.Modules;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class MapRestrictionVolumeBuilder
    {
        public static MapRestrictionVolume Make(GameObject planetGO, Sector sector, VolumesModule.VolumeInfo info)
        {
            var go = new GameObject("MapRestrictionVolume");
            go.SetActive(false);

            go.transform.parent = sector?.transform ?? planetGO.transform;
            go.transform.position = planetGO.transform.TransformPoint(info.position != null ? (Vector3)info.position : Vector3.zero);
            go.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;

            var mapRestrictionVolume = go.AddComponent<MapRestrictionVolume>();

            go.SetActive(true);

            return mapRestrictionVolume;
        }
    }
}
