using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using OWML.Utils;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class HazardVolumeBuilder
    {
        public static HazardVolume Make(GameObject planetGO, Sector sector, OWRigidbody owrb, HazardVolumeInfo info, IModBehaviour mod)
        {
            var go = GeneralPropBuilder.MakeNew("HazardVolume", planetGO, sector, info);
            go.layer = Layer.BasicEffectVolume;

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;

            var volume = AddHazardVolume(go, sector, owrb, info.type, info.firstContactDamageType, info.firstContactDamage, info.damagePerSecond);

            go.SetActive(true);

            return volume;
        }

        public static HazardVolume AddHazardVolume(GameObject go, Sector sector, OWRigidbody owrb, HazardVolumeInfo.HazardType? type, HazardVolumeInfo.InstantDamageType? firstContactDamageType, float firstContactDamage, float damagePerSecond)
        { 
            HazardVolume hazardVolume = null;
            if (type == HazardVolumeInfo.HazardType.RIVERHEAT)
            {
                hazardVolume = go.AddComponent<RiverHeatHazardVolume>();
            }
            else if (type == HazardVolumeInfo.HazardType.HEAT)
            {
                hazardVolume = go.AddComponent<HeatHazardVolume>();
            }
            else if (type == HazardVolumeInfo.HazardType.DARKMATTER)
            {
                hazardVolume = go.AddComponent<DarkMatterVolume>();
                var visorFrostEffectVolume = go.AddComponent<VisorFrostEffectVolume>();
                visorFrostEffectVolume._frostRate = 0.5f;
                visorFrostEffectVolume._maxFrost = 0.91f;

                var water = owrb.GetComponentsInChildren<RadialFluidVolume>().FirstOrDefault(x => x._fluidType == FluidVolume.Type.WATER);
                if (water != null)
                {
                    var submerge = go.AddComponent<DarkMatterSubmergeController>();
                    submerge._sector = sector;
                    submerge._effectVolumes = new EffectVolume[] { hazardVolume, visorFrostEffectVolume };
                    // THERE ARE NO RENDERERS??? RUH ROH!!!

                    var detectorGO = new GameObject("ConstantFluidDetector");
                    detectorGO.transform.parent = go.transform;
                    detectorGO.transform.localPosition = Vector3.zero;
                    detectorGO.layer = Layer.BasicDetector;
                    var detector = detectorGO.AddComponent<ConstantFluidDetector>();
                    detector._onlyDetectableFluid = water;
                    detector._buoyancy.boundingRadius = 1;
                    detector._buoyancy.checkAgainstWaves = true;
                    detector._dontApplyForces = true;

                    submerge._fluidDetector = detector;
                }
            }
            else if (type == HazardVolumeInfo.HazardType.ELECTRICITY)
            {
                var electricityVolume = go.AddComponent<ElectricityVolume>();
                electricityVolume._shockAudioPool = new OWAudioSource[0];
                hazardVolume = electricityVolume;
            }
            else
            {
                var simpleHazardVolume = go.AddComponent<SimpleHazardVolume>();
                simpleHazardVolume._type = EnumUtils.Parse(type.ToString(), HazardVolume.HazardType.GENERAL);
                hazardVolume = simpleHazardVolume;
            }
            hazardVolume._attachedBody = owrb;
            hazardVolume._damagePerSecond = type == null ? 0f : damagePerSecond;

            if (firstContactDamageType != null)
            {
                hazardVolume._firstContactDamageType = EnumUtils.Parse(firstContactDamageType.ToString(), InstantDamageType.Impact);
                hazardVolume._firstContactDamage = firstContactDamage;
            }

            return hazardVolume;
        }
    }
}
