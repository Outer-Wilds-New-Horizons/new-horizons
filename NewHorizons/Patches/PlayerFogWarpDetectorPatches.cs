using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class PlayerFogWarpDetectorPatches
    {
        // Morbius moment: they only let fog go away if there is a fog controller on the planet near you
        // However you can leave these volumes with fog on your screen, or have fog applied by a bramble node on a fogless planet

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerFogWarpDetector), nameof(PlayerFogWarpDetector.LateUpdate))]
        public static bool PlayerFogWarpDetector_LateUpdate(PlayerFogWarpDetector __instance)
        {
            if (PlanetaryFogController.GetActiveFogSphere() == null)
            {
                float num = __instance._targetFogFraction;
                if (__instance._playerEffectBubbleController != null)
                {
                    __instance._playerEffectBubbleController.SetFogFade(0, __instance._fogColor);
                }
                if (__instance._shipLandingCamEffectBubbleController != null)
                {
                    __instance._shipLandingCamEffectBubbleController.SetFogFade(0, __instance._fogColor);
                }
                return false;
            }
            return true;
        }
    }
}
