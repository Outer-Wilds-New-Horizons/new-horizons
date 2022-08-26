using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class BramblePatches
    {
        //
        // this file is not great. the real solution to the issues these patches address should be solved by replacing bramble nodes' InnerFogWarpVolume
        // components with a custom NHInnerFogWarpVolume component, and implement the below functions as overrides in the NHInnerFogWarpVolume class
        // that would fix the issue of seeds having inappropriate screen fog
        //

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SphericalFogWarpVolume), nameof(SphericalFogWarpVolume.IsProbeOnly))]
        public static bool SphericalFogWarpVolume_IsProbeOnly(SphericalFogWarpVolume __instance, ref bool __result)
        {
            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.GetFogThickness))]
        public static bool FogWarpVolume_GetFogThickness(FogWarpVolume __instance,  ref float __result)
        {
            if (__instance is InnerFogWarpVolume sph) __result = sph._exitRadius;
            else __result = 50; // 50f is hardcoded as the return value in the base game

            return false;
        }
    }
}
