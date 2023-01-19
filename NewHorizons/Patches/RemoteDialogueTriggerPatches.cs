using HarmonyLib;
using NewHorizons.Utility;
using System;

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
        [HarmonyPatch(typeof(RemoteDialogueTrigger), nameof(RemoteDialogueTrigger.OnTriggerEnter))]
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
        [HarmonyPatch(typeof(RemoteDialogueTrigger), nameof(RemoteDialogueTrigger.OnEndConversation))]
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
