using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class IslandBuilder
    {
        public static void Make(GameObject islandGO, Sector sector, OWRigidbody owrb, AstroObject primaryBody, GameObject volumesGO, PlanetConfig config, float sphereOfInfluence)
        {
            NHIslandController islandController = islandGO.AddComponent<NHIslandController>();
            islandController._planetTransform = primaryBody.transform;
            islandController._planetBody = primaryBody._owRigidbody;
            islandController._islandBody = owrb;
            islandController._safetyTractorBeams = islandGO.GetComponentsInChildren<SafetyTractorBeamController>();
            islandController._fluidDetector = islandGO.GetComponentInChildren<DynamicFluidDetector>();
            islandController._campfires = islandGO.GetComponentsInChildren<Campfire>();
            IslandAudioController islandAudioController = islandGO.AddComponent<IslandAudioController>();
            islandAudioController._sector = sector;
            islandAudioController._audioVolumes = islandGO.GetComponentsInChildren<AudioVolume>();
            islandAudioController._islandController = islandController;

            var islandObject = new GameObject("IslandVolume");
            islandObject.transform.parent = volumesGO.transform;
            islandObject.transform.localPosition = Vector3.zero;
            islandObject.transform.localScale = Vector3.one * sphereOfInfluence;
            islandObject.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphereCollider = islandObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 1;
            sphereCollider.isTrigger = true;

            var owCollider = islandObject.AddComponent<OWCollider>();
            owCollider._parentBody = owrb;
            owCollider._collider = sphereCollider;

            var triggerVolume = islandObject.AddComponent<OWTriggerVolume>();
            triggerVolume._owCollider = owCollider;

            islandController._zeroGVolume = volumesGO.GetComponentInChildren<ZeroGVolume>();
            if (islandController._zeroGVolume == null)
            {
                var zeroGVolume = islandObject.AddComponent<ZeroGVolume>();
                zeroGVolume._attachedBody = owrb;
                zeroGVolume._triggerVolume = triggerVolume;
                zeroGVolume._inheritable = true;
                zeroGVolume._priority = 1;
                islandController._zeroGVolume = zeroGVolume;
            }

            var inheritableVolume = islandObject.AddComponent<InheritibleFluidVolume>();
            inheritableVolume._attachedBody = owrb;
            inheritableVolume._triggerVolume = triggerVolume;
            inheritableVolume._alignmentFluid = false;
            inheritableVolume._priority = 3;
            inheritableVolume._density = 0;
            islandController._inheritanceFluid = inheritableVolume;

            var oceanCalmZone = islandObject.AddComponent<OceanCalmZone>();
            oceanCalmZone.fadeFactor = 0.456f;
            oceanCalmZone._ocean = primaryBody.GetComponentInChildren<OceanEffectController>();
            oceanCalmZone.localRadius = sphereOfInfluence;
            oceanCalmZone.strength = 0.73f;

            var repelFluid = islandObject.AddComponent<NHIslandRepelFluidVolume>();
            repelFluid._density = 30;
            repelFluid._layer = 5;
            repelFluid._priority = 4;
            repelFluid._fluidType = FluidVolume.Type.WATER;
            islandController._barrierRepelFluids = new FluidVolume[1] { repelFluid };

            var vanillaOceanFluid = primaryBody.GetComponentInChildren<SphereOceanFluidVolume>();
            if (vanillaOceanFluid != null)
                repelFluid.SetFluidVolume(vanillaOceanFluid);
            else
                repelFluid.SetFluidVolume(primaryBody.GetComponentInChildren<NHFluidVolume>());

            var shoreAudio = new GameObject("ShoreAudio");
            shoreAudio.transform.SetParent(sector?.transform ?? islandGO.transform, false);
            var prefab = SearchUtilities.Find("GabbroIsland_Body/Sector_GabbroIsland/ShoreAudio_GabbroIsland/ShoreAudioRail");
            var detailInfo = new External.Modules.PropModule.DetailInfo();
            var shoreAudioRail = DetailBuilder.Make(islandGO, sector, prefab, detailInfo);
            shoreAudioRail.transform.SetParent(shoreAudio.transform, true);
            islandAudioController._shoreAudioRoot = shoreAudio;
        }
    }
}
