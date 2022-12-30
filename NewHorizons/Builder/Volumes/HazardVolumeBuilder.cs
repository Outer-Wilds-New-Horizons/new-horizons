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
                    Logger.LogError($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? Vector3.zero);
            if (info.isRelativeToParent) go.transform.localPosition = pos;
            else go.transform.position = planetGO.transform.TransformPoint(pos);
            go.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;

            HazardVolume hazardVolume = null;
            if (info.type == VolumesModule.HazardVolumeInfo.HazardType.RIVERHEAT)
            {
                hazardVolume = go.AddComponent<RiverHeatHazardVolume>();
            }
            else if (info.type == VolumesModule.HazardVolumeInfo.HazardType.HEAT)
            {
                hazardVolume = go.AddComponent<HeatHazardVolume>();
            }
            else if (info.type == VolumesModule.HazardVolumeInfo.HazardType.DARKMATTER)
            {
                hazardVolume = go.AddComponent<DarkMatterVolume>();
                var visorFrostEffectVolume = go.AddComponent<VisorFrostEffectVolume>();
                visorFrostEffectVolume._frostRate = 0.5f;
                visorFrostEffectVolume._maxFrost = 0.91f;
            }
            else if (info.type == VolumesModule.HazardVolumeInfo.HazardType.ELECTRICITY)
            {
                var electricityVolume = go.AddComponent<ElectricityVolume>();
                electricityVolume._shockAudioPool = new OWAudioSource[0];
                hazardVolume = electricityVolume;
            }
            else
            {
                var simpleHazardVolume = go.AddComponent<SimpleHazardVolume>();
                simpleHazardVolume._type = EnumUtils.Parse<HazardVolume.HazardType>(info.type.ToString(), HazardVolume.HazardType.GENERAL);
                hazardVolume = simpleHazardVolume;
            }
            hazardVolume._attachedBody = owrb;
            hazardVolume._damagePerSecond = info.damagePerSecond;
            hazardVolume._firstContactDamageType = EnumUtils.Parse<InstantDamageType>(info.firstContactDamageType.ToString(), InstantDamageType.Impact);
            hazardVolume._firstContactDamage = info.firstContactDamage;

            go.SetActive(true);

            return hazardVolume;
        }
    }
}
