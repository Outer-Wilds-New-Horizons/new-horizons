using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using NewHorizons.External.Volumes;
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
        public static HazardVolume Make(GameObject planetGO, Sector sector, OWRigidbody owrb, HazardVolumeInfo info, IModBehaviour mod)
        {
            var go = GeneralPropBuilder.MakeNew("HazardVolume", sector?.transform ?? planetGO.transform, info);
            go.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;

            HazardVolume hazardVolume = null;
            if (info.type == HazardVolumeInfo.HazardType.RIVERHEAT)
            {
                hazardVolume = go.AddComponent<RiverHeatHazardVolume>();
            }
            else if (info.type == HazardVolumeInfo.HazardType.HEAT)
            {
                hazardVolume = go.AddComponent<HeatHazardVolume>();
            }
            else if (info.type == HazardVolumeInfo.HazardType.DARKMATTER)
            {
                hazardVolume = go.AddComponent<DarkMatterVolume>();
                var visorFrostEffectVolume = go.AddComponent<VisorFrostEffectVolume>();
                visorFrostEffectVolume._frostRate = 0.5f;
                visorFrostEffectVolume._maxFrost = 0.91f;

                var water = planetGO.GetComponentsInChildren<RadialFluidVolume>().FirstOrDefault(x => x._fluidType == FluidVolume.Type.WATER);
                if (water != null)
                {
                    var submerge = go.AddComponent<DarkMatterSubmergeController>();
                    submerge._sector = sector;
                    submerge._effectVolumes = new EffectVolume[] { hazardVolume, visorFrostEffectVolume };
                    // THERE ARE NO RENDERERS??? RUH ROH!!!

                    var detectorGO = new GameObject("ConstantFluidDetector");
                    detectorGO.transform.parent = go.transform;
                    detectorGO.transform.localPosition = Vector3.zero;
                    detectorGO.layer = LayerMask.NameToLayer("BasicDetector");
                    var detector = detectorGO.AddComponent<ConstantFluidDetector>();
                    detector._onlyDetectableFluid = water;
                    detector._buoyancy.boundingRadius = 1;
                    detector._buoyancy.checkAgainstWaves = true;
                    detector._dontApplyForces = true;

                    submerge._fluidDetector = detector;
                }
            }
            else if (info.type == HazardVolumeInfo.HazardType.ELECTRICITY)
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
