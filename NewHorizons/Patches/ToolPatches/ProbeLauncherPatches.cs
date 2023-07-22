using HarmonyLib;

namespace NewHorizons.Patches.ToolPatches
{
    [HarmonyPatch(typeof(ProbeLauncher))]
    public static class ProbeLauncherPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ProbeLauncher.UpdateOrbitalLaunchValues))]
        public static bool ProbeLauncher_UpdateOrbitalLaunchValues(ProbeLauncher __instance)
        {
            return Locator.GetPlayerRulesetDetector()?.GetPlanetoidRuleset()?.GetGravityVolume() != null;
        }
    }
}
