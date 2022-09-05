using NewHorizons.External.Modules;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Volumes
{
    public static class HazardVolumeBuilder
    {
        public static HazardVolume Make(GameObject planetGO, Sector sector, OWRigidbody owrb, VolumesModule.HazardVolumeInfo info, IModBehaviour mod)
        {
            var go = new GameObject("HazardVolume");
            go.SetActive(false);

            go.transform.parent = sector?.transform ?? planetGO.transform;

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

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;

            var hazardVolume = go.AddComponent<SimpleHazardVolume>();
            hazardVolume._attachedBody = owrb;
            hazardVolume._type = EnumUtils.Parse<HazardVolume.HazardType>(info.type.ToString(), HazardVolume.HazardType.GENERAL);
            hazardVolume._damagePerSecond = info.damagePerSecond;
            hazardVolume._firstContactDamageType = EnumUtils.Parse<InstantDamageType>(info.firstContactDamageType.ToString(), InstantDamageType.Impact);
            hazardVolume._firstContactDamage = info.firstContactDamage;

            go.SetActive(true);

            return hazardVolume;
        }
    }
}
