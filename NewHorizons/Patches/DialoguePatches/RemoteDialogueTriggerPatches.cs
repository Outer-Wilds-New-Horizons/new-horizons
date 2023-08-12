using HarmonyLib;

namespace NewHorizons.Patches.DialoguePatches
{
    [HarmonyPatch(typeof(RemoteDialogueTrigger))]
    public static class RemoteDialogueTriggerPatches
    {
        private static bool _wasLastDialogueInactive = false;

        /// <summary>
        /// Should fix a bug where disabled a CharacterDialogueTree makes its related RemoteDialogueTriggers softlock your game
        /// </summary>

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RemoteDialogueTrigger.OnTriggerEnter))]
        public static void RemoteDialogueTrigger_OnTriggerEnter(RemoteDialogueTrigger __instance)
        {
            if (__instance._inRemoteDialogue && __instance._activeRemoteDialogue?.gameObject != null)
            {
                _wasLastDialogueInactive = __instance._activeRemoteDialogue.gameObject.activeInHierarchy;
                if (!_wasLastDialogueInactive)
                {
                    __instance._activeRemoteDialogue.gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(RemoteDialogueTrigger.OnEndConversation))]
        public static void RemoteDialogueTrigger_OnEndConversation(RemoteDialogueTrigger __instance)
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
