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

    // TODO
    //1) fix node fog color 
    //2) test size setting for nodes/seeds and radius for dimensions
    //2) test that when a dimension has normal fog color and you shoot your probe through a seed leading to that dimension, the pictures look normal
    //3) support for existing dimensions?
    //4) signals
    //5) test whether nodes can lead to vanilla dimensions

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
                PairEntrance(nodeWarpController, dimensionName, dimensionAO);    
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

        private static bool PairEntrance(InnerFogWarpVolume nodeWarp, string destinationName, AstroObject dimensionAO = null)
        {
            var destinationAO = dimensionAO ?? AstroObjectLocator.GetAstroObject(destinationName); // find child "Sector/OuterWarp"
            if (destinationAO == null) return false;
            
            // add the destination dimension's signals to this node
            var dimensionNewHorizonsBody = destinationAO.GetComponent<NewHorizonsBody>();
            if (dimensionNewHorizonsBody != null && dimensionNewHorizonsBody.Config?.Signal?.signals != null)
            {
                var body = nodeWarp.GetComponentInParent<AstroObject>().gameObject;
                var sector = nodeWarp.GetComponentInParent<Sector>();
                
                foreach(var signalConfig in dimensionNewHorizonsBody.Config?.Signal?.signals)
                {
                    var signalGO = SignalBuilder.Make(body, sector, signalConfig, dimensionNewHorizonsBody.Mod);
                    signalGO.GetComponent<AudioSignal>()._identificationDistance = 0;
                    signalGO.transform.position = nodeWarp.transform.position;
                }        
            }

            // link the node's warp volume to the destination's
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

            var brambleSeedPrefabPath = "DB_PioneerDimension_Body/Sector_PioneerDimension/Interactables_PioneerDimension/SeedWarp_ToPioneer (1)";
            var brambleNodePrefabPath = "DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster";
            
            var path = config.isSeed ? brambleSeedPrefabPath : brambleNodePrefabPath;
            var brambleNode = DetailBuilder.MakeDetail(go, sector, path, config.position, config.rotation, config.scale, false);
            brambleNode.name = "Bramble Node to " + config.linksTo;    

            // this node comes with Feldspar's signal, we don't want that though
            GameObject.Destroy(SearchUtilities.FindChild(brambleNode, "Signal_Harmonica"));
            
            //
            // set scale
            //

            brambleNode.transform.localScale = Vector3.one * config.scale;

            //
            // change the colors
            //
            
            if (config.isSeed) SetSeedColors(brambleNode, config.fogTint.ToColor(), config.lightTint.ToColor());
            else               SetNodeColors(brambleNode, config.fogTint.ToColor(), config.lightTint.ToColor());

            //
            // set up warps
            //

            var warpController = brambleNode.GetComponent<InnerFogWarpVolume>();
            warpController._sector = sector;
            warpController._attachedBody = go.GetComponent<OWRigidbody>(); // I don't think this is necessary, it seems to be set correctly on its own
            warpController._containerWarpVolume = GetOuterFogWarpVolumeFromAstroObject(go); // the OuterFogWarpVolume of the dimension this node is inside of (null if this node is not inside of a bramble dimension (eg it's sitting on a planet or something))
            var success = PairEntrance(warpController, config.linksTo);
            if (!success) RecordUnpairedNode(warpController, config.linksTo);

            warpController.Awake(); // I can't spawn this game object disabled, but Awake needs to run after _sector is set. That means I need to call Awake myself

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

        public static void SetNodeColors(GameObject brambleNode, Color fogTint, Color lightTint)
        {
            var effects = SearchUtilities.FindChild(brambleNode, "Effects");
            var fogRenderer = SearchUtilities.FindChild(effects, "InnerWarpFogSphere").GetComponent<OWRenderer>();
            var lightShafts = SearchUtilities.FindChild(effects, "DB_BrambleLightShafts");

            if (fogTint != null) 
            { 
                //var fogMeshRenderer = fogRenderer.GetComponent<MeshRenderer>();
                //var mat = fogMeshRenderer.material;
                //mat.color = config.fogTint.ToColor();
                //fogMeshRenderer.sharedMaterial = mat;
                
                Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => fogRenderer.SetColor(fogTint), 10); 
            } 

            if (lightTint != null)
            {
                var lightShaft1 = SearchUtilities.FindChild(lightShafts, "BrambleLightShaft1");
                var mat = lightShaft1.GetComponent<MeshRenderer>().material;
                mat.color = lightTint;
                
                for (int i = 1; i <= 6; i++)
                {
                    var lightShaft = SearchUtilities.FindChild(lightShafts, $"BrambleLightShaft{i}");
                    lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }
        }

        public static void SetSeedColors(GameObject brambleSeed, Color fogTint, Color lightTint)
        {
            var fogRenderer = SearchUtilities.FindChild(brambleSeed, "VolumetricFogSphere (2)");
            var effects = SearchUtilities.FindChild(brambleSeed, "Terrain_DB_BrambleSphere_Seed_V2 (2)");
            var lightShafts = SearchUtilities.FindChild(effects, "DB_SeedLightShafts");

            if (fogTint != null) 
            { 
                var fogMeshRenderer = fogRenderer.GetComponent<MeshRenderer>();
                var mat = fogMeshRenderer.material;
                mat.color = fogTint;
                fogMeshRenderer.sharedMaterial = mat;
            } 
            
            if (lightTint != null)
            {
                var lightShaft1 = SearchUtilities.FindChild(lightShafts, "DB_SeedLightShafts1");
                var mat = lightShaft1.GetComponent<MeshRenderer>().material;
                mat.color = lightTint;
                
                for (int i = 1; i <= 6; i++)
                {
                    var lightShaft = SearchUtilities.FindChild(lightShafts, $"DB_SeedLightShafts{i}");
                    lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }
        }
    }
}
