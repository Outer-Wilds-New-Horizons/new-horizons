using HarmonyLib;

namespace NewHorizons.Patches.DialoguePatches
{
    [HarmonyPatch(typeof(HearthianRecorderEffects))]
    public static class HearthianRecorderEffectsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(HearthianRecorderEffects.Awake))]
        public static bool HearthianRecorderEffects_Awake(HearthianRecorderEffects __instance)
        {
            // If we're adding custom dialogue to a recorder the CharacterDialogueTree isn't going to be on the object
            if (__instance.GetComponent<CharacterDialogueTree>() == null)
            {
                __instance.enabled = false;
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
