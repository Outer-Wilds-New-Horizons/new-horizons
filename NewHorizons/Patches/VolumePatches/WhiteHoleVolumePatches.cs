using HarmonyLib;
using System;
using System.Collections.Generic;

namespace NewHorizons.Patches.VolumePatches
{
    [HarmonyPatch(typeof(WhiteHoleVolume))]
    public static class WhiteHoleVolumePatches
    {
        // To fix custom white holes
        [HarmonyPrefix]
        [HarmonyPatch(nameof(WhiteHoleVolume.Awake))]
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
    }
}
