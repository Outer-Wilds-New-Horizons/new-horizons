using HarmonyLib;

namespace NewHorizons.Patches
{
    /// <summary>
    /// Should fix a bug where disabled a CharacterDialogueTree makes its related RemoteDialogueTriggers softlock your game
    /// </summary>
    [HarmonyPatch]
    public static class RemoteDialogueTriggerPatches
    {
        private static bool _wasLastDialogueInactive = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDMarker), nameof(HUDMarker.Awake))]
        public static void OnTriggerEnter(RemoteDialogueTrigger __instance)
        {
            if (__instance._inRemoteDialogue && __instance._activeRemoteDialogue != null)
            {
                _wasLastDialogueInactive = __instance._activeRemoteDialogue.gameObject.activeInHierarchy;
                __instance._activeRemoteDialogue.gameObject.SetActive(true);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDMarker), nameof(HUDMarker.Awake))]
        public static void OnEndConversation(RemoteDialogueTrigger __instance)
        {
            if (__instance._inRemoteDialogue && __instance._activeRemoteDialogue != null)
            {
                if (_wasLastDialogueInactive)
                {
                    __instance._activeRemoteDialogue.gameObject.SetActive(false);
                }
            }

            _wasLastDialogueInactive = false;
        }
    }
}
