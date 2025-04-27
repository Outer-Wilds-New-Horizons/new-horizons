using HarmonyLib;
using UnityEngine;
using static AnglerfishController;

namespace NewHorizons.Patches
{
    [HarmonyPatch(typeof(QuantumMoon))]
    public static class QuantumMoonPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(QuantumMoon.CheckIllumination))]
        public static void QuantumMoon_CheckIllumination(QuantumMoon __instance, ref bool __result)
        {
            if (__result)
            {
                __result = !PlayerState.InDarkZone();
            }
        }
    }
}
