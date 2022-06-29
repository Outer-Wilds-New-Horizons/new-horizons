using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Body
{
    public class BrambleDimensionBuilder
    {
        public void Make()
        {
            var dimensionPrefab = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension");
            var dimension = GameObject.Instantiate(dimensionPrefab);
            //dimension.name = config.rename ?? "Custom Bramble Dimension";
            
            var dimensionSector = SearchUtilities.FindChild(dimension, "Sector_HubDimension");
            dimensionSector.name = "Sector";
            var atmo = SearchUtilities.FindChild(dimension, "Atmosphere_HubDimension");
            var geom = SearchUtilities.FindChild(dimension, "Geometry_HubDimension");
            var vols = SearchUtilities.FindChild(dimension, "Volumes_HubDimension");
            var efxs = SearchUtilities.FindChild(dimension, "Effects_HubDimension");
            var intr = SearchUtilities.FindChild(dimension, "Interactables_HubDimension");
            var exitWarps = SearchUtilities.FindChild(intr, "OuterWarp_Hub");

            exitWarps.transform.parent = dimensionSector.transform;
            atmo.name = "Atmosphere";
            geom.name = "Geometry"; // disable this?
            vols.name = "Volumes";
            efxs.name = "Effects";
            intr.name = "Interactibles";
            GameObject.Destroy(intr);

            // TODO: set "exitWarps/ExitPoint", "exitWarp/ExitPoint (1)", ... "exitWarp/ExitPoint (5)"
        }
    }
}
