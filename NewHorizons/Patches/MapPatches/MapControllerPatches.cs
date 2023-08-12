using HarmonyLib;
using NewHorizons.Handlers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(MapController))]
    public static class MapControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapController.Start))]
        public static void MapController_Start(MapController __instance)
        {
            var modifier = Mathf.Max(1f, PlanetCreationHandler.SolarSystemRadius / PlanetCreationHandler.DefaultFurthestOrbit);

            __instance._maxPanDistance *= modifier;
            __instance._maxZoomDistance *= modifier;
            __instance._zoomSpeed *= modifier;
            __instance._mapCamera.farClipPlane *= modifier * 4f;

            if (Main.SystemDict[Main.Instance.CurrentStarSystem].Config.freeMapAngle)
            {
                __instance._minPitchAngle = -90f;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapController.OnTargetReferenceFrame))]
        public static void MapController_OnTargetReferenceFrame(MapController __instance, ReferenceFrame referenceFrame)
        {
            // Locked onto map satellite just means it will move vertically up from the plane of the solar system
            __instance._isLockedOntoMapSatellite = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MapController.MapInoperable))]
        public static bool MapController_MapInoperable(MapController __instance, ref bool __result)
        {
            if (SceneManager.GetActiveScene().name != "SolarSystem") return true;

            try
            {
                if (Main.SystemDict[Main.Instance.CurrentStarSystem].Config.mapRestricted)
                {
                    __instance._playerMapRestricted = true;
                    __result = true;
                    return false;
                }
            }
            catch { }

            return true;
        }
    }
}
