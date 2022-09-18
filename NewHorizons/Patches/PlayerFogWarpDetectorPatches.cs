using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class PlayerFogWarpDetectorPatches
    {
        // Morbius moment: they only let fog go away if there is a fog controller on the planet near you
        // However you can leave these volumes with fog on your screen, or have fog applied by a bramble node on a fogless planet

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerFogWarpDetector), nameof(PlayerFogWarpDetector.LateUpdate))]
        public static void PlayerFogWarpDetector_LateUpdate(PlayerFogWarpDetector __instance)
        {
            if (PlanetaryFogController.GetActiveFogSphere() == null)
            {
                __instance._fogFraction = 0;
                if (__instance._playerEffectBubbleController != null)
                {
                    __instance._playerEffectBubbleController.SetFogFade(__instance._fogFraction, __instance._fogColor);
                }
                if (__instance._shipLandingCamEffectBubbleController != null)
                {
                    __instance._shipLandingCamEffectBubbleController.SetFogFade(__instance._fogFraction, __instance._fogColor);
                }
            }
        }
    }
}
