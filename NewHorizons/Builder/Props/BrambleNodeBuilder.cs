using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public class BrambleNodeBuilder
    {
        

            // DB_EscapePodDimension_Body/Sector_EscapePodDimension/Interactables_EscapePodDimension/InnerWarp_ToAnglerNest // need to change the light shaft color
            // DB_ExitOnlyDimension_Body/Sector_ExitOnlyDimension/Interactables_ExitOnlyDimension/InnerWarp_ToExitOnly  // need to change the colors
            // DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster   // need to delete the child "Signal_Harmonica"

        public void Make(GameObject go, Sector sector, BrambleNodeConfig config)
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
            // warpController._linkedOuterWarpVolume = the outer warp volume of the dimension this node leads to
            // warpController._containerWarpVolume = the OuterFogWarpVolume of the dimension this node is inside of (null if this node is not inside of a bramble dimension (eg it's sitting on a planet or something))

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
        }
    }
}
