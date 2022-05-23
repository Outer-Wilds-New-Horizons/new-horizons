using HarmonyLib;
using System;
using System.Collections.Generic;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class SingularityPatches
    {
        // For our custom black holes that don't link to anything
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BlackHoleVolume), nameof(BlackHoleVolume.Start))]
        public static bool BlackHoleVolume_Start(BlackHoleVolume __instance)
        {
            return __instance._whiteHole == null;
        }

        // To fix custom white holes
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WhiteHoleVolume), nameof(WhiteHoleVolume.Awake))]
        public static bool WhiteHoleVolume_Awake(WhiteHoleVolume __instance)
        {
            __instance._growQueue = new List<OWRigidbody>(8);
            __instance._growQueueLocationData = new List<RelativeLocationData>(8);
            __instance._ejectedBodyList = new List<OWRigidbody>(64);
            try
            {
                __instance._whiteHoleBody = __instance.gameObject.GetAttachedOWRigidbody(false);
                __instance._whiteHoleProxyShadowSuperGroup = __instance._whiteHoleBody.GetComponentInChildren<ProxyShadowCasterSuperGroup>();
                __instance._fluidVolume = __instance.gameObject.GetRequiredComponent<WhiteHoleFluidVolume>();
            }
            catch (Exception) { }
            return false;
        }

        // This is to stop the game throwing too many errors if the probe is destroyed by a blackhole
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SurveyorProbe), nameof(SurveyorProbe.IsLaunched))]
        public static bool SurveyorProbe_IsLaunched(SurveyorProbe __instance, ref bool __result)
        {
            try
            {
                __result = __instance.gameObject.activeSelf;
            }
            catch (Exception)
            {
                __result = true;
            }
            return false;
        }
    }
}
