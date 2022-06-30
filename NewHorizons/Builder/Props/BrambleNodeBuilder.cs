using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NewHorizons.External.Modules.BrambleModule;

namespace NewHorizons.Builder.Props
{
    public static class BrambleNodeBuilder
    {
        // keys are all dimension names that have been referenced by at least one node but do not (yet) exist
        // values are all nodes' warp controllers that link to a given dimension
        // unpairedNodes[name of dimension that doesn't exist yet] => List{warp controller for node that links to that dimension, ...}
        private static Dictionary<string, List<InnerFogWarpVolume>> unpairedNodes = new();

        public static void PairUnpairedNodesForDimension(string dimensionName, AstroObject dimensionAO = null)
        {
            if (!unpairedNodes.ContainsKey(dimensionName)) return;

            foreach (var warpVolume in unpairedNodes[dimensionName])
            {
                Pair(warpVolume, dimensionName, dimensionAO);    
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
            //
            // spawn the bramble node
            //

            var brambleNodePrefabPath = "DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster";
            var brambleNode = DetailBuilder.MakeDetail(go, sector, brambleNodePrefabPath, config.position, config.rotation, config.scale, false);
            
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
