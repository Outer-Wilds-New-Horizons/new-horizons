using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using OWML.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class HazardVolumeBuilder
    {
        public static HazardVolume Make(GameObject planetGO, Sector sector, OWRigidbody owrb, HazardVolumeInfo info, IModBehaviour mod)
        {
            var go = GeneralPropBuilder.MakeNew("HazardVolume", planetGO, sector, info);
            
            var volume = MakeExisting(go, planetGO, sector, owrb, info);

            go.SetActive(true);

            return volume;
        }

        public static HazardVolume MakeExisting(GameObject go, GameObject planetGO, Sector sector, OWRigidbody owrb, HazardVolumeInfo info)
        { 
            HazardVolume hazardVolume = null;
            if (info.type == HazardVolumeInfo.HazardType.RIVERHEAT)
            {
                hazardVolume = VolumeBuilder.MakeExisting<RiverHeatHazardVolume>(go, planetGO, sector, info);
            }
            else if (info.type == HazardVolumeInfo.HazardType.HEAT)
            {
                hazardVolume = VolumeBuilder.MakeExisting<HeatHazardVolume>(go, planetGO, sector, info);
            }
            else if (info.type == HazardVolumeInfo.HazardType.DARKMATTER)
            {
                hazardVolume = VolumeBuilder.MakeExisting<DarkMatterVolume>(go, planetGO, sector, info);
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
            else if (info.type == HazardVolumeInfo.HazardType.ELECTRICITY)
            {
                var electricityVolume = VolumeBuilder.MakeExisting<ElectricityVolume>(go, planetGO, sector, info);
                electricityVolume._shockAudioPool = new OWAudioSource[0];
                hazardVolume = electricityVolume;
            }
            else
            {
                var simpleHazardVolume = go.AddComponent<SimpleHazardVolume>();
                simpleHazardVolume._type = EnumUtils.Parse(info.type.ToString(), HazardVolume.HazardType.GENERAL);
                hazardVolume = simpleHazardVolume;
            }
            hazardVolume._attachedBody = owrb;
            hazardVolume._damagePerSecond = info.type == HazardVolumeInfo.HazardType.NONE ? 0f : info.damagePerSecond;

            hazardVolume._firstContactDamageType = EnumUtils.Parse(info.firstContactDamageType.ToString(), InstantDamageType.Impact);
            hazardVolume._firstContactDamage = info.firstContactDamage;

            return hazardVolume;
        }

        public static HazardVolume AddHazardVolume(GameObject go, Sector sector, OWRigidbody owrb, HazardVolumeInfo.HazardType? type, HazardVolumeInfo.InstantDamageType? firstContactDamageType, float firstContactDamage, float damagePerSecond)
        {
            var planetGO = sector.transform.root.gameObject;
            return MakeExisting(go, planetGO, sector, owrb, new HazardVolumeInfo
            {
                radius = 0f, // Volume builder should skip creating an extra trigger volume and collider if radius is 0
                type = type ?? HazardVolumeInfo.HazardType.NONE,
                firstContactDamageType = firstContactDamageType ?? HazardVolumeInfo.InstantDamageType.Impact,
                firstContactDamage = firstContactDamage,
                damagePerSecond = damagePerSecond
            });
        }
    }
}
