using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class PlayerStatePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerState), nameof(PlayerState.CheckShipOutsideSolarSystem))]
        public static bool PlayerState_CheckShipOutsideSolarSystem(PlayerState __instance, ref bool __result)
        {
            if (PlayerState._inBrambleDimension) return false;

            // Stop the game from trying to recall your ship when you're visiting far away planets

            Transform sunTransform = Locator.GetSunTransform();
            OWRigidbody shipBody = Locator.GetShipBody();
            var maxDist2 = Mathf.Max(900000000f, Main.FurthestOrbit * Main.FurthestOrbit * 2f);
            __result = sunTransform != null && shipBody != null && (sunTransform.position - shipBody.transform.position).sqrMagnitude > maxDist2;
            return false;
        }
    }
}
