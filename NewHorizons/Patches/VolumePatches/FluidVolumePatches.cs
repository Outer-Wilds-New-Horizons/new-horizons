using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches.VolumePatches
{
    [HarmonyPatch(typeof(FluidVolume))]
    public static class FluidVolumePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FluidVolume.GetDepthAtPosition))]
        public static bool FluidVolume_GetDepthAtPosition(FluidVolume __instance, ref float __result, Vector3 worldPosition)
        {
            if (__instance is not RadialFluidVolume radialFluidVolume) return true;

            Vector3 vector = radialFluidVolume.transform.InverseTransformPoint(worldPosition);
            __result = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z + vector.y * vector.y) - radialFluidVolume._radius;
            return false;
        }
    }
}
