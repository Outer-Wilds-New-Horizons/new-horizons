using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NewHorizons.External.Modules.BrambleModule;

namespace NewHorizons.Builder.Body
{
    public static class BrambleDimensionBuilder
    {
        public static GameObject Make(NewHorizonsBody body)
        {
            var config = body.Config.Bramble.dimension;

            // spawn the dimension body
            var dimensionPrefab = SearchUtilities.Find("DB_HubDimension_Body");
            var dimension = GameObject.Instantiate(dimensionPrefab);
            var ao = dimension.GetComponent<AstroObject>();

            // fix name
            var name = body.Config.name ?? "Custom Bramble Dimension";
            ao._customName = name;
            ao._name = AstroObject.Name.CustomString;
            dimension.name = name.Replace(" ", "").Replace("'", "") + "_Body";

            // TODO: radius (need to determine what the base radius is first)

            // fix children's names and remove base game props (mostly just bramble nodes that are children to Interactibles) and set up the OuterWarp child
            var dimensionSector = SearchUtilities.FindChild(dimension, "Sector_HubDimension");
            dimensionSector.name = "Sector";
            var atmo = SearchUtilities.FindChild(dimension, "Atmosphere_HubDimension");
            var geom = SearchUtilities.FindChild(dimension, "Geometry_HubDimension");
            var vols = SearchUtilities.FindChild(dimension, "Volumes_HubDimension");
            var efxs = SearchUtilities.FindChild(dimension, "Effects_HubDimension");
            var intr = SearchUtilities.FindChild(dimension, "Interactables_HubDimension");
            var exitWarps = SearchUtilities.FindChild(intr, "OuterWarp_Hub");

            exitWarps.name = "OuterWarp";
            exitWarps.transform.parent = dimensionSector.transform;
            atmo.name = "Atmosphere";
            geom.name = "Geometry"; // disable this?
            vols.name = "Volumes";
            efxs.name = "Effects";
            intr.name = "Interactibles";
            GameObject.Destroy(intr);

            exitWarps.GetComponent<OuterFogWarpVolume>()._senderWarps.Clear();
            exitWarps.GetComponent<OuterFogWarpVolume>()._linkedInnerWarpVolume = null;

            // TODO MAYBE: set "exitWarps/ExitPoint", "exitWarp/ExitPoint (1)", ... "exitWarp/ExitPoint (5)"

            return dimension;
        }
    }
}
