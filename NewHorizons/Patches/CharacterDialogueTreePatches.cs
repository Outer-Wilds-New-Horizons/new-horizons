using HarmonyLib;

namespace NewHorizons.Patches;

[HarmonyPatch]
internal class CharacterDialogueTreePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.Awake))]
    private static void CharacterDialogueTree_Awake(CharacterDialogueTree __instance)
    {
        GlobalMessenger<OWRigidbody>.AddListener("AttachPlayerToPoint", (_) => __instance.EndConversation());
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.OnDestroy))]
    private static void CharacterDialogueTree_OnDestroy(CharacterDialogueTree __instance)
    {
        GlobalMessenger<OWRigidbody>.RemoveListener("AttachPlayerToPoint", (_) => __instance.EndConversation());
    }
}
