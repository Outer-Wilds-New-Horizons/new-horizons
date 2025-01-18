using HarmonyLib;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using System.Linq;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(CosmicInflationController))]
    public static class CosmicInflationControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CosmicInflationController.UpdateFormation))]
        public static void CosmicInflationController_UpdateFormation(CosmicInflationController __instance)
        {
            if (__instance._waitForCrossfade && EyeSceneHandler.GetCustomEyeTravelers().Any())
            {
                NHLogger.Log($"Hijacking finale cross-fade, NH will handle it");
                __instance._waitForCrossfade = false;
                __instance._waitForMusicEnd = true;
                EyeSceneHandler.GetMusicController().TransitionToFinale();
            }
        }
    }
}
