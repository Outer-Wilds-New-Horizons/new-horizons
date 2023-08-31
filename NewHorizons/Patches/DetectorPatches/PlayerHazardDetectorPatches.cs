using HarmonyLib;
using NewHorizons.Utility.OWML;

namespace NewHorizons.Patches.DetectorPatches
{
    [HarmonyPatch(typeof(HazardDetector))]
    public static class PlayerHazardDetectorPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HazardDetector.Awake))]
        public static void HazardDetector_Awake(HazardDetector __instance)
        {
            // Prevent the player detector from being hurt while the solar system is being set up
            if (__instance._isPlayerDetector && !Main.IsSystemReady)
            {
                __instance.enabled = false;
                Delay.RunWhen(() => Main.IsSystemReady, () => __instance.enabled = true);
            }
        }
    }
}
