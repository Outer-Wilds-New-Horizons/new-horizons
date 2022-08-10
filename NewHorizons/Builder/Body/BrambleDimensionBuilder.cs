using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.Components.Orbital;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    // TODO: in order to fix fog screen effect for scaling nodes, I need to replace all InnerFogWarpVolume and OuterFogWarpVolume instances with NHInner/OuterFogWarpVolume and on those two classes, implement GetFogThickness(){ return 50*scale; }}
    // TODO: StreamingHandler.SetUpStreaming() for all FogWarpEffectBubbleController objects
    // TODO: add the "don't see me" effect 


    // Patch PlayerFogWarpDetector.LateUpdate to figure out why the screen fog isn't working

    // try FogWarpBubbleController.SetFogFade?

    public static class BrambleDimensionBuilder
    {
        public static readonly float BASE_DIMENSION_RADIUS = 750f;

        // location of all vanilla bramble dimensions
        //-9116.795 -19873.44 2480.327
        //-8460.688 -19873.44 6706.444
        //-5015.165 -19873.44 4142.816
        //-8993.414 -17059.44 4521.747
        //-7044.813 -17135.44 3272.149
        //-6904.48  -17048.44 5574.479
        //-11096.95 -22786.44 4657.534
        //-8716.807 -22786.44 4496.394


        // keys are all node names that have been referenced as an exit by at least one dimension but do not (yet) exist
        // values are all dimensions' warp controllers that link to a given dimension
        // unpairedNodes[name of node that doesn't exist yet] => List{warp controller for dimension that exits to that node, ...}
        private static Dictionary<string, List<OuterFogWarpVolume>> _unpairedDimensions;

        public static void Init()
        {
            // Just in case something went wrong and a dimension never got paired last time
            _unpairedDimensions = new();
        }

        public static GameObject Make(NewHorizonsBody body, GameObject go, NHAstroObject ao, Sector sector, OWRigidbody owRigidBody)
        {
            var config = body.Config.Bramble.dimension;

            ao.IsDimension = true;
            sector._name = Sector.Name.BrambleDimension;

            var atmo = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension/Atmosphere_HubDimension").InstantiateInactive();
            var volumes = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension/Volumes_HubDimension").InstantiateInactive();
            var effects = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension/Effects_HubDimension").InstantiateInactive();

            var prefab = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension/Geometry_HubDimension");
            var detailInfo = new PropModule.DetailInfo();
            var geometry = DetailBuilder.MakeDetail(go, sector, prefab, detailInfo);

            var exitWarps = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/OuterWarp_Hub").InstantiateInactive();
            var repelVolume = SearchUtilities.Find("DB_HubDimension_Body/BrambleRepelVolume").InstantiateInactive();

            atmo.name = "Atmosphere";
            atmo.transform.parent = sector.transform;
            atmo.transform.localPosition = Vector3.zero;

            volumes.name = "Volumes";
            volumes.transform.parent = sector.transform;
            volumes.transform.localPosition = Vector3.zero;

            effects.name = "Effects";
            effects.transform.parent = sector.transform;
            effects.transform.localPosition = Vector3.zero;

            geometry.name = "Geometry";
            geometry.transform.parent = sector.transform;
            geometry.transform.localPosition = Vector3.zero;

            exitWarps.name = "OuterWarp";
            exitWarps.transform.parent = sector.transform;
            exitWarps.transform.localPosition = Vector3.zero;

            repelVolume.name = "BrambleRepelVolume";
            repelVolume.transform.parent = sector.transform;
            repelVolume.transform.localPosition = Vector3.zero;

            // TODO: Remove default vines (once we create an asset bundle version of the outer sphere that has collisions
            /*
            var geoBatchedGroup = geometry.FindChild("BatchedGroup");
            var collider = geoBatchedGroup.FindChild("BatchedMeshColliders_0");
            collider.transform.parent = geometry.transform;
            GameObject.Destroy(geoBatchedGroup);

            var geoOtherComponentsGroup = geometry.FindChild("OtherComponentsGroup");
            var dimensionWalls = geoOtherComponentsGroup.FindChild("Terrain_DB_BrambleSphere_Outer_v2");
            dimensionWalls.transform.parent = geometry.transform;
            GameObject.Destroy(geoOtherComponentsGroup);
            */

            // fix some cull groups
            volumes.GetComponent<SectorCollisionGroup>()._sector = sector;
            volumes.FindChild("SunOverrideVolume").GetComponent<SunOverrideVolume>()._sector = sector;
            effects.GetComponent<SectorCullGroup>()._sector = sector;
            atmo.GetComponent<SectorCullGroup>()._sector = sector;
            atmo.GetComponent<SectorLightsCullGroup>()._sector = sector;
            
            // Set up rulesets
            var thrustRuleset = sector.gameObject.AddComponent<ThrustRuleset>();
            thrustRuleset._attachedBody = owRigidBody;
            thrustRuleset._triggerVolume = sector.GetTriggerVolume();
            thrustRuleset._nerfDuration = 0.5f;
            thrustRuleset._nerfJetpackBooster = false;
            thrustRuleset._thrustLimit = 20;

            var effectRuleset = sector.gameObject.AddComponent<EffectRuleset>();
            effectRuleset._attachedBody = owRigidBody;
            effectRuleset._triggerVolume = sector.GetTriggerVolume();
            effectRuleset._type = EffectRuleset.BubbleType.FogWarp;
            effectRuleset._underwaterDistortScale = 0.001f;
            effectRuleset._underwaterMaxDistort = 0.1f;
            effectRuleset._underwaterMinDistort = 0.005f;
            effectRuleset._material = GameObject.Find("DB_PioneerDimension_Body/Sector_PioneerDimension").GetComponent<EffectRuleset>()._material; // TODO: cache this

            var antiTravelMusicRuleset = sector.gameObject.AddComponent<AntiTravelMusicRuleset>();
            antiTravelMusicRuleset._attachedBody = owRigidBody;
            antiTravelMusicRuleset._triggerVolume = sector.GetTriggerVolume();

            // Set up warps
            var outerFogWarpVolume = exitWarps.GetComponent<OuterFogWarpVolume>();
            outerFogWarpVolume._senderWarps = new List<InnerFogWarpVolume>();
            outerFogWarpVolume._linkedInnerWarpVolume = null;
            outerFogWarpVolume._name = OuterFogWarpVolume.Name.None;
            outerFogWarpVolume._sector = sector;

            PairExit(config.linksTo, outerFogWarpVolume);

            // If the config says only certain entrances are allowed, enforce that
            if (config.allowedEntrances != null)
            {
                var entrances = outerFogWarpVolume._exits;
                var newEntrances = new List<SphericalFogWarpExit>();
                foreach (var index in config.allowedEntrances)
                {
                    if(index is < 0 or > 5) continue;
                    newEntrances.Add(entrances[index]);
                }
                outerFogWarpVolume._exits = newEntrances.ToArray();
            }

            // Set the scale
            var scale = config.radius / BASE_DIMENSION_RADIUS;
            geometry.transform.localScale = Vector3.one * scale;
            sector.gameObject.GetComponent<SphereShape>().radius *= scale;
            outerFogWarpVolume._warpRadius *= scale;
            outerFogWarpVolume._exitRadius *= scale;
            
            var fogGO = atmo.FindChild("FogSphere_Hub");
            var fog = fogGO.GetComponent<PlanetaryFogController>();
            fog._fogRadius *= scale;
            fog._fogDensity = config.fogDensity * scale;
            atmo.FindChild("FogBackdrop_Hub").transform.localScale *= scale;

            var volumesShape = volumes.FindChild("ZeroG_Fluid_Audio_Volume");
            var sphereShape = volumesShape.GetComponent<SphereShape>();
            sphereShape.enabled = true; // this starts disabled for some fucking reason
            sphereShape.radius *= scale;

            // Change fog color
            if (config.fogTint != null)
            {
                var color = config.fogTint.ToColor();
                fog.fogTint = color;
                outerFogWarpVolume._fogColor = color;
            }

            // Set up repel volume and cloak to scale and only contain this dimension
            // The base game one is on the HUB dimension and encompasses all bramble dimensions and their sectors
            repelVolume.GetComponent<SphereShape>().radius = 2400f * scale;
            repelVolume.GetComponent<DarkBrambleRepelVolume>()._innerRadius = 2010f * scale;
            var cloak = repelVolume.GetComponentInChildren<DarkBrambleCloakSphere>();
            cloak.transform.localScale = Vector3.one * 4020f * scale;
            cloak._sectors = new Sector[] { sector };
            cloak.GetComponent<Renderer>().enabled = true;

            // Cull stuff
            var cullController = go.AddComponent<BrambleSectorController>();
            cullController.SetSector(sector);

            // finalize
            atmo.SetActive(true);
            volumes.SetActive(true);
            effects.SetActive(true);
            geometry.SetActive(true);
            exitWarps.SetActive(true);
            repelVolume.SetActive(true);

            return go;
        }

        public static void PairExit(string exitName, OuterFogWarpVolume warpController)
        {
            if (!BrambleNodeBuilder.namedNodes.ContainsKey(exitName))
            {
                if (!_unpairedDimensions.ContainsKey(exitName)) _unpairedDimensions[exitName] = new();
                _unpairedDimensions[exitName].Add(warpController);
                return;
            }
            
            warpController._linkedInnerWarpVolume = BrambleNodeBuilder.namedNodes[exitName];
        }

        public static void FinishPairingDimensionsForExitNode(string nodeName)
        {
            if (!_unpairedDimensions.ContainsKey(nodeName)) return;

            var warpControllers = _unpairedDimensions[nodeName].ToList();
            foreach (var dimensionWarpController in warpControllers)
            {
                PairExit(nodeName, dimensionWarpController);    
            }

            //unpairedDimensions.Remove(nodeName);
        }

    }
}
