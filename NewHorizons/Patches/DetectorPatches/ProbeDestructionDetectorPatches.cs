using HarmonyLib;
using NewHorizons.OtherMods.AchievementsPlus.NH;
using NewHorizons.OtherMods.AchievementsPlus;
using UnityEngine;

namespace NewHorizons.Patches.DetectorPatches
{
    [HarmonyPatch(typeof(ProbeDestructionDetector))]
    internal static class ProbeDestructionDetectorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ProbeDestructionDetector.FixedUpdate))]
        public static bool ProbeDestructionDetector_FixedUpdate(ProbeDestructionDetector __instance)
        {
            if (__instance._activeVolumes.Count > 0 && __instance._safetyVolumes.Count == 0)
            {
                // Mobius does SetConditionState even when you are in solar system because probe never get destroyed anywhere else but the Eye.
                if (LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse)
                {
                    DialogueConditionManager.SharedInstance.SetConditionState("PROBE_ENTERED_EYE", conditionState: true);
                    Debug.Log("PROBE DESTROYED (ENTERED THE EYE)");
                }
                else
                    Debug.Log("PROBE DESTROYED");

                if (AchievementHandler.Enabled) ProbeLostAchievement.Earn();

                Object.Destroy(__instance._probe.gameObject);
            }
            __instance.enabled = false;
            return false;
        }
    }
}
