using HarmonyLib;
using NewHorizons.Builder.Props;
using UnityEngine;

namespace NewHorizons.Patches.VolumePatches
{
    [HarmonyPatch]
    public static class FogWarpVolumePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SphericalFogWarpVolume), nameof(SphericalFogWarpVolume.IsProbeOnly))]
        public static bool SphericalFogWarpVolume_IsProbeOnly(SphericalFogWarpVolume __instance, ref bool __result)
        {
            // Do not affect base game volumes
            if (!BrambleNodeBuilder.IsNHFogWarpVolume(__instance))
            {
                return true;
            }

            // Check the ratio between these to determine if seed, instead of just < 10
            __result = Mathf.Approximately(__instance._exitRadius / __instance._warpRadius, 2f);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.GetFogThickness))]
        public static bool FogWarpVolume_GetFogThickness(FogWarpVolume __instance, ref float __result)
        {
            // Do not affect base game volumes
            if (!BrambleNodeBuilder.IsNHFogWarpVolume(__instance))
            {
                return true;
            }

            if (__instance is InnerFogWarpVolume sph)
            {
                __result = sph._exitRadius;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
