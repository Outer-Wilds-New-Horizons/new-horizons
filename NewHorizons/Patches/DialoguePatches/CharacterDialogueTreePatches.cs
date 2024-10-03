using HarmonyLib;
using NewHorizons.OtherMods.AchievementsPlus.NH;
using NewHorizons.OtherMods.AchievementsPlus;
using NewHorizons.Components.Props;

namespace NewHorizons.Patches.DialoguePatches;

[HarmonyPatch(typeof(CharacterDialogueTree))]
public static class CharacterDialogueTreePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CharacterDialogueTree.Awake))]
    private static void CharacterDialogueTree_Awake(CharacterDialogueTree __instance)
    {
        GlobalMessenger<OWRigidbody>.AddListener("AttachPlayerToPoint", __instance.OnAttachPlayerToPoint);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CharacterDialogueTree.OnDestroy))]
    private static void CharacterDialogueTree_OnDestroy(CharacterDialogueTree __instance)
    {
        GlobalMessenger<OWRigidbody>.RemoveListener("AttachPlayerToPoint", __instance.OnAttachPlayerToPoint);
    }

    private static void OnAttachPlayerToPoint(this CharacterDialogueTree characterDialogueTree, OWRigidbody rigidbody)
    {
        if (characterDialogueTree.InConversation())
        {
            characterDialogueTree.EndConversation();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(CharacterDialogueTree.StartConversation))]
    public static void CharacterDialogueTree_StartConversation(CharacterDialogueTree __instance)
    {
        if (!AchievementHandler.Enabled) return;

        if (__instance is NHCharacterDialogueTree)
        {
            TalkToFiveCharactersAchievement.OnTalkedToCharacter(__instance._characterName);
        }
    }
}
