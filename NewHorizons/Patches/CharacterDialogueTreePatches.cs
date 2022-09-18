using HarmonyLib;

namespace NewHorizons.Patches;

[HarmonyPatch]
internal static class CharacterDialogueTreePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.Awake))]
    private static void CharacterDialogueTree_Awake(CharacterDialogueTree __instance)
    {
        GlobalMessenger<OWRigidbody>.AddListener("AttachPlayerToPoint", __instance.OnAttachPlayerToPoint);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.OnDestroy))]
    private static void CharacterDialogueTree_OnDestroy(CharacterDialogueTree __instance)
    {
        GlobalMessenger<OWRigidbody>.RemoveListener("AttachPlayerToPoint", __instance.OnAttachPlayerToPoint);
    }

    private static void OnAttachPlayerToPoint(this CharacterDialogueTree characterDialogueTree, OWRigidbody rigidbody)
    {
        characterDialogueTree.EndConversation();
    }
}
