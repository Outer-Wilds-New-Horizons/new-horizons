using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class ProbeLauncherPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UpdateOrbitalLaunchValues))]
        public static bool ProbeLauncher_UpdateOrbitalLaunchValues(ProbeLauncher __instance) =>
            Locator.GetPlayerRulesetDetector()?.GetPlanetoidRuleset()?.GetGravityVolume() != null;
    }
}