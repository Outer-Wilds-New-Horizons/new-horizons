using HarmonyLib;
using System.Collections;
using UnityEngine.InputSystem;

namespace NewHorizons.Patches.DialoguePatches
{
    [HarmonyPatch(typeof(RemoteDialogueTrigger))]
    public static class RemoteDialogueTriggerPatches
    {
        private static bool _wasLastDialogueInactive = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RemoteDialogueTrigger.Awake))]
        public static void RemoteDialogueTrigger_Awake(RemoteDialogueTrigger __instance)
        {
            // Wait for player to be up and moving before allowing them to trigger remote dialogue
            // Stops you getting locked into dialogue while waking up
            if (OWInput.GetInputMode() != InputMode.Character)
            {
                __instance._collider.enabled = false;
                __instance.StartCoroutine(AwakeCoroutine(__instance));
            }
        }

        private static IEnumerator AwakeCoroutine(RemoteDialogueTrigger instance)
        {
            while (OWInput.GetInputMode() != InputMode.Character)
            {
                yield return null;
            }

            instance._collider.enabled = true;
        }

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
