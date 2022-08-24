using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches
{
    /// <summary>
    /// Don't allow anything to be destroyed. Just disable them instead.
    /// </summary>
    [HarmonyPatch]
    public static class DestroyPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestroyOnStart), nameof(DestroyOnStart.Start))]
        private static bool DestroyOnStart_Start(DestroyOnStart __instance)
        {
            __instance.gameObject.SetActive(false);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestroyOnCollision), nameof(DestroyOnCollision.Update))]
        private static bool DestroyOnCollision_Update(DestroyOnCollision __instance)
        {
            if (Time.time > __instance._collideTime + __instance._destroyDelay) __instance.gameObject.SetActive(false);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestroyOnEnterTrigger), nameof(DestroyOnEnterTrigger.OnEntry))]
        private static bool DestroyOnEnterTrigger_OnEntry(DestroyOnEnterTrigger __instance, GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector")) __instance.gameObject.SetActive(false);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestroyOnEvent), nameof(DestroyOnEvent.OnStartOfTimeLoop))]
        private static bool DestroyOnEvent_OnStartOfTimeLoop(DestroyOnEvent __instance, int loopCount)
        {
            if ((__instance._startOfFirstTimeLoop && loopCount < 2) || (__instance._onLoopGO3 && loopCount > 1)) __instance.gameObject.SetActive(false);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestroyOnEvent), nameof(DestroyOnEvent.OnResumeSimulation))]
        private static bool DestroyOnEvent_OnResumeSimulation(DestroyOnEvent __instance)
        {
            if (__instance._resumeSimulation) __instance.gameObject.SetActive(false);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DestroyOnEvent), nameof(DestroyOnEvent.OnNomaiStatueActivated))]
        private static bool DestroyOnEvent_OnNomaiStatueActivated(DestroyOnEvent __instance)
        {
            if (__instance._nomaiStatueActivated) __instance.gameObject.SetActive(false);
            return false;
        }
    }
}
