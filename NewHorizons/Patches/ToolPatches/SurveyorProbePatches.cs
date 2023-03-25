using HarmonyLib;

namespace NewHorizons.Patches.ToolPatches
{
    [HarmonyPatch(typeof(SurveyorProbe))]
    public static class SurveyorProbePatches
    {
        // This is to stop the game throwing too many errors if the probe is destroyed by a blackhole
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SurveyorProbe.IsLaunched))]
        public static bool SurveyorProbe_IsLaunched(SurveyorProbe __instance, out bool __result)
        {
            __result = __instance?.gameObject?.activeSelf ?? false;
            return false;
        }
    }
}
