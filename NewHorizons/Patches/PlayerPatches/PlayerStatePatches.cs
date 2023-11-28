using HarmonyLib;
using NewHorizons.Handlers;
using UnityEngine;
namespace NewHorizons.Patches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerState))]
    public static class PlayerStatePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerState.CheckShipOutsideSolarSystem))]
        public static bool PlayerState_CheckShipOutsideSolarSystem(PlayerState __instance, ref bool __result)
        {
            if (PlayerState._inBrambleDimension) return false;

            // Stop the game from trying to recall your ship when you're visiting far away planets

            var centerTransform = Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().transform;
            var shipBody = Locator.GetShipBody();
            var maxDist = Mathf.Max(PlanetCreationHandler.DefaultFurthestOrbit, PlanetCreationHandler.SolarSystemRadius);
            __result = centerTransform != null && shipBody != null && (shipBody.transform.position - centerTransform.position).sqrMagnitude > maxDist * maxDist;
            return false;
        }
    }
}
