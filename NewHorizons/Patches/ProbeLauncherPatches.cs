#region

using HarmonyLib;

#endregion

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class ProbeLauncherPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UpdateOrbitalLaunchValues))]
        public static bool ProbeLauncher_UpdateOrbitalLaunchValues(ProbeLauncher __instance)
        {
            return Locator.GetPlayerRulesetDetector()?.GetPlanetoidRuleset()?.GetGravityVolume() != null;
        }
    }
}