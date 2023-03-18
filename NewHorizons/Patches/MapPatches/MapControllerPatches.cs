using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(MapController))]
    public static class MapControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapController.Awake))]
        public static void MapController_Awake(MapController __instance)
        {
            __instance._maxPanDistance = Mathf.Max(__instance._maxPanDistance, Main.FurthestOrbit * 1.5f);
            __instance._maxZoomDistance *= 6f;
            __instance._minPitchAngle = -90f;
            __instance._zoomSpeed *= 4f;
            __instance._mapCamera.farClipPlane = Mathf.Max(__instance._mapCamera.farClipPlane, Main.FurthestOrbit * 10f);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapController.OnTargetReferenceFrame))]
        public static void MapController_OnTargetReferenceFrame(MapController __instance, ReferenceFrame __0)
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
