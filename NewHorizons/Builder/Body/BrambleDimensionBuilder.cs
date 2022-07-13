using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    [HarmonyPatch]
    static class Patch
    {
        // SkinnedMeshRenderer.SetBlendShapeWeight
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerFogWarpDetector), nameof(PlayerFogWarpDetector.LateUpdate))]
        public static bool LateUpdate(PlayerFogWarpDetector __instance)
        {
      //      if (PlanetaryFogController.GetActiveFogSphere() == null)
		    //{
			   // return false;
		    //}
		    float num = __instance._targetFogFraction;
		    if (PlayerState.IsInsideShip())
		    {
			    num = Mathf.Max(__instance._shipFogDetector.GetTargetFogFraction(), num);
		    }
		    if (num < __instance._fogFraction)
		    {
			    float num2 = (__instance._closestFogWarp.UseFastFogFade() ? 1f : 0.2f);
			    __instance._fogFraction = Mathf.MoveTowards(__instance._fogFraction, num, Time.deltaTime * num2);
		    }
		    else
		    {
			    __instance._fogFraction = num;
		    }
		    if (__instance._targetFogColorWarpVolume != __instance._closestFogWarp)
		    {
			    __instance._targetFogColorWarpVolume = __instance._closestFogWarp;
			    __instance._startColorCrossfadeTime = Time.time;
			    __instance._startCrossfadeColor = __instance._fogColor;
		    }
		    if (__instance._targetFogColorWarpVolume != null)
		    {
			    Color fogColor = __instance._targetFogColorWarpVolume.GetFogColor();
			    if (__instance._fogFraction <= 0f)
			    {
				    __instance._fogColor = fogColor;
			    }
			    else
			    {
				    float t = Mathf.InverseLerp(__instance._startColorCrossfadeTime, __instance._startColorCrossfadeTime + 1f, Time.time);
				    __instance._fogColor = Color.Lerp(__instance._startCrossfadeColor, fogColor, t);
			    }
                __instance._fogColor = new Color(__instance._fogColor.r, __instance._fogColor.g, __instance._fogColor.b);
		    }
		    if (__instance._playerEffectBubbleController != null)
		    {
			    __instance._playerEffectBubbleController.SetFogFade(__instance._fogFraction, __instance._fogColor);
		    }
		    if (__instance._shipLandingCamEffectBubbleController != null)
		    {
			    __instance._shipLandingCamEffectBubbleController.SetFogFade(__instance._fogFraction, __instance._fogColor);
		    }

            return false;
        }


        // FogWarpBubbleController.SetFogFade
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpEffectBubbleController), nameof(FogWarpEffectBubbleController.SetFogFade))]
	    public static bool FogWarpBubbleController_SetFogFade(FogWarpEffectBubbleController __instance, float fogAlpha, Color fogColor)
	    {
		    if (__instance._effectBubbleRenderer.sharedMaterial != null)
		    {
			    Color value = fogColor;
			    value.a = fogAlpha;
			    __instance._matPropBlock.SetColor(__instance._propID_Color, value);
			    __instance._effectBubbleRenderer.SetPropertyBlock(__instance._matPropBlock);
		    }
		    __instance._visible = __instance._effectBubbleRenderer.sharedMaterial != null && fogAlpha > 0f;
		    if (__instance._targetCamera == null)
		    {
			    __instance._effectBubbleRenderer.enabled = __instance._visible;
                Logger.Log($"Setting camera effect renderer to {__instance._visible}");
		    }

            // Logger.Log($"{__instance.gameObject.transform.GetPath()}        _visible: {__instance._visible}  set alpha to {fogAlpha}  and set color ot {fogColor}");

            return false;
	    }

        
        // note: I would just make this a one line postfix function, but CheckWarpProximity() does extra stuff that we really don't want to run twice
        // so we have to completely override this function to support scaling ;-;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpDetector), nameof(FogWarpDetector.FixedUpdate))]
	    public static void FixedUpdate(FogWarpDetector __instance)
	    {
		    __instance._targetFogFraction = 0f;
		    __instance._closestFogWarp = null;
		    float num = float.PositiveInfinity;
		    for (int i = 0; i < __instance._warpVolumes.Count; i++)
		    {
			    if (!__instance._warpVolumes[i].IsProbeOnly() || __instance._name == FogWarpDetector.Name.Probe)
			    {
				    FogWarpVolume fogWarpVolume = __instance._warpVolumes[i];
				    float num2 = Mathf.Abs(fogWarpVolume.CheckWarpProximity(__instance)); 
				    float b = Mathf.Clamp01(1f - Mathf.Abs(num2) / fogWarpVolume.GetFogThickness());
				    __instance._targetFogFraction = Mathf.Max(__instance._targetFogFraction, b);
				    if (num2 < num)
				    {
					    num = num2;
					    __instance._closestFogWarp = fogWarpVolume;
				    }
			    }
		    }
	    }
    }

    // TODO: in order to fix fog screen effect for scaling nodes, I need to replace all InnerFogWarpVolume and OuterFogWarpVolume instances with NHInner/OuterFogWarpVolume and on those two classes, implement GetFogThickness(){ return 50*scale; }}
    // TODO: StreamingHandler.SetUpStreaming() for all FogWarpEffectBubbleController objects
    // TODO: add the "don't see me" effect 


    // Patch PlayerFogWarpDetector.LateUpdate to figure out why the screen fog isn't working

    // try FogWarpBubbleController.SetFogFade?

    public static class BrambleDimensionBuilder
    {
        public static readonly float BASE_DIMENSION_RADIUS = 1705f;

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
            var geometry = DetailBuilder.MakeDetail(go, sector, "DB_HubDimension_Body/Sector_HubDimension/Geometry_HubDimension", Vector3.zero, Vector3.zero, 1, false);
            var exitWarps = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension/OuterWarp_Hub").InstantiateInactive();
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

            // Set the scale
            var scale = config.radius / BASE_DIMENSION_RADIUS;
            geometry.transform.localScale = Vector3.one * scale;
            outerFogWarpVolume._warpRadius *= scale;
            outerFogWarpVolume._exitRadius *= scale;
            
            var fogGO = atmo.FindChild("FogSphere_Hub");
            var fog = fogGO.GetComponent<PlanetaryFogController>();
            fog._fogRadius *= scale;
            fog._fogDensity *= scale;

            // Change fog color
            if (body.Config.Bramble.dimension.fogTint != null)
            {
                fog.fogTint = body.Config.Bramble.dimension.fogTint.ToColor();
            }

            // Set up repel volume to only contain this dimension
            // The base game one is on the HUB dimension and encompasses all bramble dimensions and their sectors
            var cloak = repelVolume.gameObject.GetComponentInChildren<DarkBrambleCloakSphere>();
            cloak.transform.localScale = Vector3.one * 4000f;
            cloak._sectors = new Sector[] { sector };

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
            if (!BrambleNodeBuilder.NamedNodes.ContainsKey(exitName))
            {
                if (!_unpairedDimensions.ContainsKey(exitName)) _unpairedDimensions[exitName] = new();
                _unpairedDimensions[exitName].Add(warpController);
                return;
            }
            
            warpController._linkedInnerWarpVolume = BrambleNodeBuilder.NamedNodes[exitName];
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
