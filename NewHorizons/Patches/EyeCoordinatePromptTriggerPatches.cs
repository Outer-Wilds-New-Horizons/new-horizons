using HarmonyLib;
using NewHorizons.Handlers;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class EyeCoordinatePromptTriggerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EyeCoordinatePromptTrigger), nameof(EyeCoordinatePromptTrigger.Update))]
        public static void EyeCoordinatePromptTrigger_Update(EyeCoordinatePromptTrigger __instance)
        {
            var showPrompts = __instance._warpController.HasPower();

            VesselCoordinatePromptHandler.SetPromptVisibility(showPrompts);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EyeCoordinatePromptTrigger), nameof(EyeCoordinatePromptTrigger.OnExit))]
        public static void EyeCoordinatePromptTrigger_OnExit(GameObject __0)
        {
            if (__0.CompareTag("PlayerDetector"))
            {
                VesselCoordinatePromptHandler.SetPromptVisibility(false);
            }
        }
    }
}
