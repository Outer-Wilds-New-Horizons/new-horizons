using HarmonyLib;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(ShipLogMapMode))]
    public static class ShipLogMapModePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ShipLogMapMode.Initialize))]
        public static void ShipLogMapMode_Initialize(ShipLogMapMode __instance)
        {
            GameObject panRoot = SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH);
            GameObject sunObject = SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/Sun");
            ShipLogAstroObject[][] navMatrix = MapModeBuilder.ConstructMapMode(Main.Instance.CurrentStarSystem, panRoot, __instance._astroObjects, sunObject.layer);
            // Remove anything that isnt meant to be selectable
            var flagRemovedUnselectable = false;
            for (int i = 0; i < navMatrix.Length; i++)
            {
                for (int j = 0; j < navMatrix[i].Length; j++)
                {
                    if (!(MapModeBuilder.GetMapModeInfoForAstroObject(navMatrix[i][j])?.selectable ?? true))
                    {
                        flagRemovedUnselectable = true;
                        navMatrix[i][j].UpdateState();
                        navMatrix[i][j] = null;
                    }
                }
            }
            if (flagRemovedUnselectable)
            {
                navMatrix = navMatrix.Where(a => a.Count(c => c != null) > 0).ToArray();
                for (var index = 0; index < navMatrix.Length; index++)
                {
                    navMatrix[index] = navMatrix[index].Where(a => a != null).ToArray();
                }
            }

            if (navMatrix == null || navMatrix.Length <= 1)
            {
                NHLogger.LogWarning("Skipping Map Mode Generation.");
            }
            else
            {
                __instance._astroObjects = navMatrix;
                __instance._startingAstroObjectID = Main.SystemDict[Main.Instance.CurrentStarSystem].Config.shipLogStartingPlanetID ?? navMatrix[1][0].GetID();
                if (Main.Instance.CurrentStarSystem != "SolarSystem")
                {
                    List<GameObject> delete = panRoot.GetAllChildren().Where(g => g.name.Contains("_ShipLog") == false).ToList();
                    foreach (GameObject gameObject in delete)
                    {
                        Object.Destroy(SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + gameObject.name));
                    }
                    if (SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + "SandFunnel") == null)
                    {
                        __instance._sandFunnel = __instance.gameObject.AddComponent<ShipLogSandFunnel>();
                    }
                }
            }

            NHLogger.Log("Map Mode Construction Complete");
        }
    }
}
