using HarmonyLib;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(CosmicInflationController))]
    public static class CosmicInflationControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CosmicInflationController.UpdateFormation))]
        public static void CosmicInflationController_UpdateFormation(CosmicInflationController __instance)
        {
            if (__instance._waitForCrossfade)
            {
                NHLogger.Log($"Hijacking finale cross-fade, NH will handle it");
                __instance._waitForCrossfade = false;
                __instance._waitForMusicEnd = true;
                EyeSceneHandler.GetMusicController().TransitionToFinale();
            }
        }
    }
}
