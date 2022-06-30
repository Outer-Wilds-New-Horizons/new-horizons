using HarmonyLib;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NewHorizons.External.Modules.BrambleModule;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    // Issue: these nodes aren't getting added to the list PlayerFogWarpDetector._warpVolumes
    // debugging: try overriding FogWarpDetector.TrackFogWarpVolume(FogWarpVolume volume) to see if it's even getting added to this list at all
     [HarmonyPatch]
    public static class FogDebuggingPatches
    {
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.OnOccupantEnterSector))]
        private static bool FogWarpVolume_OnOccupantEnterSector(FogWarpVolume __instance, SectorDetector detector)
        {
            Logger.LogWarning($"Warp volume {__instance.name} is attempting to get sector detector {detector.name} to register it");
            FogWarpDetector component = detector.GetComponent<FogWarpDetector>();
		    if (component != null)
		    {
                Logger.LogWarning("FogWarpDetector component was found");
			    component.TrackFogWarpVolume(__instance);
		    }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpDetector), nameof(FogWarpDetector.TrackFogWarpVolume))]
        public static bool FogWarpDetector_TrackFogWarpVolume(FogWarpDetector __instance, FogWarpVolume volume)
        {
            Logger.LogWarning($"Detector {__instance.name} is attempting to track fog warp volume {volume.name}");
            bool flag = false;
		    if (!__instance._warpVolumes.SafeAdd(volume))
		    {
                Logger.LogError("Failed to add warp volume to tracking list");
			    return false;
		    }
		    __instance.enabled = true;
		    if (volume.IsOuterWarpVolume())
		    {
			    if (__instance._outerWarpVolume != null)
			    {
				    Logger.LogError("Entering an outer warp volume before leaving the old one!");
				    //Debug.Break();
			    }
			    if (__instance._outerWarpVolume != volume)
			    {
				    flag = true;
			    }
			    __instance._outerWarpVolume = (OuterFogWarpVolume)volume;
		    }
		    //if (__instance.OnTrackFogWarpVolume != null)
		    //{
			   // __instance.OnTrackFogWarpVolume(volume);
		    //}
		    //if (flag && __instance.OnOuterFogWarpVolumeChange != null)
		    //{
			   // __instance.OnOuterFogWarpVolumeChange(__instance._outerWarpVolume);
		    //}

            return false;
        }
    }

    public static class BrambleNodeBuilder
    {
        // keys are all dimension names that have been referenced by at least one node but do not (yet) exist
        // values are all nodes' warp controllers that link to a given dimension
        // unpairedNodes[name of dimension that doesn't exist yet] => List{warp controller for node that links to that dimension, ...}
        private static Dictionary<string, List<InnerFogWarpVolume>> unpairedNodes = new();

        public static Dictionary<string, InnerFogWarpVolume> namedNodes = new();

        public static void FinishPairingNodesForDimension(string dimensionName, AstroObject dimensionAO = null, BrambleDimensionInfo dimensionInfo = null)
        {
            Logger.Log("Pairing for " + dimensionName);
            // TODO: I might need to call this on Make: InnerFogWarpVolume.OnOccupantEnterSector

            // pair node->dimension (entrances)
            if (unpairedNodes.ContainsKey(dimensionName))
            {
                foreach (var nodeWarpController in unpairedNodes[dimensionName])
                {
                    Pair(nodeWarpController, dimensionName, dimensionAO);    
                }

                unpairedNodes.Remove(dimensionName);
            }

            // pair dimension->node (exit)
            if (dimensionInfo != null && dimensionAO != null && namedNodes.ContainsKey(dimensionInfo.linksTo))
            {
                var dimensionWarpController = dimensionAO.GetComponentInChildren<OuterFogWarpVolume>();
                dimensionWarpController._linkedInnerWarpVolume = namedNodes[dimensionInfo.linksTo];
            }
        }

        private static void RecordUnpairedNode(InnerFogWarpVolume warpVolume, string linksTo)
        {
            if (!unpairedNodes.ContainsKey(linksTo)) unpairedNodes[linksTo] = new();
            
            unpairedNodes[linksTo].Add(warpVolume);
        }

        private static OuterFogWarpVolume GetOuterFogWarpVolumeFromAstroObject(GameObject go)
        {
            var sector = SearchUtilities.FindChild(go, "Sector");
            if (sector == null) return null;

            var outerWarpGO = SearchUtilities.FindChild(sector, "OuterWarp");
            if (outerWarpGO == null) return null;

            var outerFogWarpVolume = outerWarpGO.GetComponent<OuterFogWarpVolume>();
            return outerFogWarpVolume;
        }

        private static bool Pair(InnerFogWarpVolume nodeWarp, string destinationName, AstroObject dimensionAO = null)
        {
            var destinationAO = dimensionAO ?? AstroObjectLocator.GetAstroObject(destinationName); // find child "Sector/OuterWarp"
            if (destinationAO == null) return false;

            var destination = GetOuterFogWarpVolumeFromAstroObject(destinationAO.gameObject);
            if (destination == null) return false;

            nodeWarp._linkedOuterWarpVolume = destination;
            destination._senderWarps.Add(nodeWarp);
            return true;
        }


        // DB_EscapePodDimension_Body/Sector_EscapePodDimension/Interactables_EscapePodDimension/InnerWarp_ToAnglerNest // need to change the light shaft color
        // DB_ExitOnlyDimension_Body/Sector_ExitOnlyDimension/Interactables_ExitOnlyDimension/InnerWarp_ToExitOnly  // need to change the colors
        // DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster   // need to delete the child "Signal_Harmonica"

        public static void Make(GameObject go, Sector sector, BrambleNodeInfo[] configs)
        {
            foreach(var config in configs)
            {
                Make(go, sector, config);
            }
        }

        public static GameObject Make(GameObject go, Sector sector, BrambleNodeInfo config)
        {
            Logger.LogError($"Building node {config.name}");

            //
            // spawn the bramble node
            //

            var brambleNodePrefabPath = "DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster";
            var brambleNode = DetailBuilder.MakeDetail(go, sector, brambleNodePrefabPath, config.position, config.rotation, config.scale, false);
            brambleNode.name = "Bramble Node to " + config.linksTo;    

            // this node comes with Feldspar's signal, we don't want that though
            GameObject.Destroy(SearchUtilities.FindChild(brambleNode, "Signal_Harmonica"));
            
            //
            // TODO: change the colors
            //

            //var effects = SearchUtilities.FindChild(brambleNode, "Effects");
            //var fogRenderer = SearchUtilities.FindChild(effects, "InnerWarpFogSphere");
            //var lightShafts = SearchUtilities.FindChild(effects, "DB_BrambleLightShafts");

            //var lightShaft1 = SearchUtilities.FindChild(lightShafts, "BrambleLightShaft1");

            //
            // set up warps
            //

            var warpController = brambleNode.GetComponent<InnerFogWarpVolume>();
            warpController._sector = sector;
            warpController._attachedBody = go.GetComponent<OWRigidbody>(); // I don't think this is necessary, it seems to be set correctly on its own
            warpController._containerWarpVolume = GetOuterFogWarpVolumeFromAstroObject(go); // the OuterFogWarpVolume of the dimension this node is inside of (null if this node is not inside of a bramble dimension (eg it's sitting on a planet or something))
            var success = Pair(warpController, config.linksTo);
            if (!success) RecordUnpairedNode(warpController, config.linksTo);

            warpController.Awake(); // I can't spawn this game object disabled, but Awake needs to run after _sector is set. That means I need to call Awake myself

            //var exitPointsParent = SearchUtilities.FindChild(brambleNode, "FogWarpExitPoints"); // "ExitPoint", "ExitPoint (1)" ... "ExitPoint (5)"
            //var exitPointsNames = new string[] 
            //{ 
            //    "ExitPoint", 
            //    "ExitPoint (1)", 
            //    "ExitPoint (2)", 
            //    "ExitPoint (3)", 
            //    "ExitPoint (4)", 
            //    "ExitPoint (5)", 
            //};
            //for (int i = 0; i < 6; i++)
            //{
            //    var exitPoint = SearchUtilities.FindChild(exitPointsParent, exitPointsNames[i]);
            //    var sphericalFogWarpExit = exitPoint.GetComponent<SphericalFogWarpExit>();
            //    // I don't think anything actually needs to be done here
            //}
            
            //
            // TODO: support adding signals to these nodes
            //
            
            // Done!
            return brambleNode;
        }
    }
}
