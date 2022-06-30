using HarmonyLib;
using NewHorizons.Builder.Body;
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
    
    // [HarmonyPatch]
    //public static class FogDebuggingPatches
    //{

        
    //    [HarmonyPrefix]
    //    [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.WarpDetector))]
	   // public static bool FogWarpVolume_WarpDetector(FogWarpVolume __instance, FogWarpDetector detector, FogWarpVolume linkedWarpVolume)
	   // {
		  //  bool flag = detector.CompareName(FogWarpDetector.Name.Player);
		  //  bool flag2 = detector.CompareName(FogWarpDetector.Name.Ship);
		  //  if (!flag || !PlayerState.IsInsideShip())
		  //  {
			 //   OWRigidbody oWRigidbody = detector.GetOWRigidbody();
			 //   if (flag && PlayerState.IsAttached())
			 //   {
				//    oWRigidbody = detector.GetOWRigidbody().transform.parent.GetComponentInParent<OWRigidbody>();
				//    MonoBehaviour.print("body to warp: " + oWRigidbody.name);
			 //   }
			 //   Vector3 localRelVelocity = __instance.transform.InverseTransformDirection(oWRigidbody.GetVelocity() - __instance._attachedBody.GetVelocity());
			 //   Vector3 localPos = __instance.transform.InverseTransformPoint(oWRigidbody.transform.position);
			 //   Quaternion localRot = Quaternion.Inverse(__instance.transform.rotation) * oWRigidbody.transform.rotation;
			 //   if (flag2 && PlayerState.IsInsideShip())
			 //   {
				//    __instance._sector.GetTriggerVolume().RemoveObjectFromVolume(Locator.GetPlayerDetector());
				//    __instance._sector.GetTriggerVolume().RemoveObjectFromVolume(Locator.GetPlayerCameraDetector());
			 //   }
			 //   if (flag || (flag2 && PlayerState.IsInsideShip()))
			 //   {
				//    GlobalMessenger.FireEvent("PlayerFogWarp");
			 //   }
			 //   __instance._sector.GetTriggerVolume().RemoveObjectFromVolume(detector.gameObject);
			 //   linkedWarpVolume.ReceiveWarpedDetector(detector, localRelVelocity, localPos, localRot);
			 //   //if (__instance.OnWarpDetector != null)
			 //   //{
				//   // __instance.OnWarpDetector(detector);
			 //   //}
		  //  }

    //        return false;
	   // }

        
    //    [HarmonyPrefix]
    //    [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.OnOccupantEnterSector))]
    //    private static bool FogWarpVolume_OnOccupantEnterSector(FogWarpVolume __instance, SectorDetector detector)
    //    {
    //        Logger.LogWarning($"Warp volume {__instance.name} is attempting to get sector detector {detector.name} to register it");
    //        FogWarpDetector component = detector.GetComponent<FogWarpDetector>();
		  //  if (component != null)
		  //  {
    //            Logger.LogWarning("FogWarpDetector component was found");
			 //   component.TrackFogWarpVolume(__instance);
		  //  }

    //        return false;
    //    }

    //    [HarmonyPrefix]
    //    [HarmonyPatch(typeof(FogWarpDetector), nameof(FogWarpDetector.TrackFogWarpVolume))]
    //    public static bool FogWarpDetector_TrackFogWarpVolume(FogWarpDetector __instance, FogWarpVolume volume)
    //    {
    //        Logger.LogWarning($"Detector {__instance.name} is attempting to track fog warp volume {volume.name}");
    //        bool flag = false;
		  //  if (!__instance._warpVolumes.SafeAdd(volume))
		  //  {
    //            Logger.LogError("Failed to add warp volume to tracking list");
			 //   return false;
		  //  }
		  //  __instance.enabled = true;
		  //  if (volume.IsOuterWarpVolume())
		  //  {
			 //   if (__instance._outerWarpVolume != null)
			 //   {
				//    Logger.LogError("Entering an outer warp volume before leaving the old one!");
				//    //Debug.Break();
			 //   }
			 //   if (__instance._outerWarpVolume != volume)
			 //   {
				//    flag = true;
			 //   }
			 //   __instance._outerWarpVolume = (OuterFogWarpVolume)volume;
		  //  }
		  //  //if (__instance.OnTrackFogWarpVolume != null)
		  //  //{
			 //  // __instance.OnTrackFogWarpVolume(volume);
		  //  //}
		  //  //if (flag && __instance.OnOuterFogWarpVolumeChange != null)
		  //  //{
			 //  // __instance.OnOuterFogWarpVolumeChange(__instance._outerWarpVolume);
		  //  //}

    //        return false;
    //    }
    //}

    public static class BrambleNodeBuilder
    {
        // keys are all dimension names that have been referenced by at least one node but do not (yet) exist
        // values are all nodes' warp controllers that link to a given dimension
        // unpairedNodes[name of dimension that doesn't exist yet] => List{warp controller for node that links to that dimension, ...}
        private static Dictionary<string, List<InnerFogWarpVolume>> unpairedNodes = new();

        public static Dictionary<string, InnerFogWarpVolume> namedNodes = new();

        public static void FinishPairingNodesForDimension(string dimensionName, AstroObject dimensionAO = null)
        {
            if (!unpairedNodes.ContainsKey(dimensionName)) return;

            foreach (var nodeWarpController in unpairedNodes[dimensionName])
            {
                Pair(nodeWarpController, dimensionName, dimensionAO);    
            }

            unpairedNodes.Remove(dimensionName);
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
            destination.RegisterSenderWarp(nodeWarp);
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

            var effects = SearchUtilities.FindChild(brambleNode, "Effects");
            var fogRenderer = SearchUtilities.FindChild(effects, "InnerWarpFogSphere").GetComponent<OWRenderer>();
            var lightShafts = SearchUtilities.FindChild(effects, "DB_BrambleLightShafts");


            if (config.fogTint != null) fogRenderer.SetColor(config.fogTint.ToColor());
            if (config.lightTint != null)
            {
                var lightShaft1 = SearchUtilities.FindChild(lightShafts, "BrambleLightShaft1");
                var mat = lightShaft1.GetComponent<MeshRenderer>().material;
                mat.color = config.lightTint.ToColor();
                
                for (int i = 1; i <= 6; i++)
                {
                    var lightShaft = SearchUtilities.FindChild(lightShafts, $"BrambleLightShaft{i}");
                    lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }

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
            
            //
            // Cleanup for dimension exits
            //
            if (config.name != null)
            {
                namedNodes[config.name] = warpController;
                BrambleDimensionBuilder.FinishPairingDimensionsForExitNode(config.name);
            }


            // Done!
            return brambleNode;
        }
    }
}
