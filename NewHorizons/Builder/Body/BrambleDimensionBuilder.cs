using NewHorizons.Builder.Props;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
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

        public static GameObject Make(NewHorizonsBody body)
        {
            var config = body.Config.Bramble.dimension;

            // spawn the dimension body
            var dimensionPrefab = SearchUtilities.Find("DB_HubDimension_Body");
            var dimension = dimensionPrefab.InstantiateInactive();
            
            // Fix AO
            var ao = dimension.GetComponent<AstroObject>();
            var nhao = dimension.AddComponent<NHAstroObject>();
            nhao.CopyPropertiesFrom(ao);
            Component.Destroy(ao);

            nhao.IsDimension = true;
            var name = body.Config.name ?? "Custom Bramble Dimension";
            nhao._customName = name;
            nhao._name = AstroObject.Name.CustomString;
            dimension.name = name.Replace(" ", "").Replace("'", "") + "_Body";

            // fix children's names and remove base game props (mostly just bramble nodes that are children to Interactibles) and set up the OuterWarp child
            var dimensionSector = dimension.FindChild("Sector_HubDimension");
            dimensionSector.name = "Sector";
            var atmo = dimensionSector.FindChild("Atmosphere_HubDimension");
            var geom = dimensionSector.FindChild("Geometry_HubDimension");
            var vols = dimensionSector.FindChild("Volumes_HubDimension");
            var efxs = dimensionSector.FindChild("Effects_HubDimension");
            var intr = dimensionSector.FindChild("Interactables_HubDimension");
            var exitWarps = intr.FindChild("OuterWarp_Hub");

            exitWarps.name = "OuterWarp";
            exitWarps.transform.parent = dimensionSector.transform;
            atmo.name = "Atmosphere";
            geom.name = "Geometry"; // disable this?
            vols.name = "Volumes";
            efxs.name = "Effects";
            intr.name = "Interactibles";
            GameObject.Destroy(intr);

            // Set up warps
            var outerFogWarpVolume = exitWarps.GetComponent<OuterFogWarpVolume>();
            outerFogWarpVolume._senderWarps = new List<InnerFogWarpVolume>();
            outerFogWarpVolume._linkedInnerWarpVolume = null;
            outerFogWarpVolume._name = OuterFogWarpVolume.Name.None;

            PairExit(config.linksTo, outerFogWarpVolume);

            // change fog color
            if (body.Config.Bramble.dimension.fogTint != null)
            {
                var fogGO = atmo.FindChild("FogSphere_Hub");
                var fog = fogGO.GetComponent<PlanetaryFogController>();
                fog.fogTint = body.Config.Bramble.dimension.fogTint.ToColor();
            }

            dimension.SetActive(true);

            return dimension;
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
