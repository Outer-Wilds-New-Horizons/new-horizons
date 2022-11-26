using HarmonyLib;
using NewHorizons.Components;
using NewHorizons.OtherMods.AchievementsPlus.NH;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class AchievementPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeDestructionDetector), nameof(ProbeDestructionDetector.FixedUpdate))]
        public static bool ProbeDestructionDetector_FixedUpdate(ProbeDestructionDetector __instance)
        {
            if (__instance._activeVolumes.Count > 0 && __instance._safetyVolumes.Count == 0)
            {
                if (LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse)
                {
                    DialogueConditionManager.SharedInstance.SetConditionState("PROBE_ENTERED_EYE", conditionState: true);
                    Debug.Log("PROBE DESTROYED (ENTERED THE EYE)");
                }
                else
                    Debug.Log("PROBE DESTROYED");

                ProbeLostAchievement.Earn();
                Object.Destroy(__instance._probe.gameObject);
            }
            __instance.enabled = false;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.StartConversation))]
        public static void CharacterDialogueTree_StartConversation(CharacterDialogueTree __instance)
        {
            if (__instance is NHCharacterDialogueTree)
            {
                TalkToFiveCharactersAchievement.OnTalkedToCharacter(__instance._characterName);
            }
        }
    }
}
