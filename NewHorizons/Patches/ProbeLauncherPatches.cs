using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class ProbeLauncherPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.UpdateOrbitalLaunchValues))]
        public static bool ProbeLauncher_UpdateOrbitalLaunchValues(ProbeLauncher __instance)
        {
            return (Locator.GetPlayerRulesetDetector()?.GetPlanetoidRuleset()?.GetGravityVolume() != null);
        }
    }
}
