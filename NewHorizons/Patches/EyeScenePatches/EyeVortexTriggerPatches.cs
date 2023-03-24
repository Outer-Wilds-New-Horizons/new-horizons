using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(EyeVortexTrigger))]
    public static class EyeVortexTriggerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(EyeVortexTrigger.OnEnterVortex))]
        public static void EyeVortexTrigger_OnEnterVortex(EyeVortexTrigger __instance, GameObject hitObj)
        {
            if (!hitObj.CompareTag("PlayerDetector")) return;
            __instance._tunnelObject.SetActive(true);
        }
    }
}
