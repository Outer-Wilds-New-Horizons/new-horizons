using HarmonyLib;

namespace NewHorizons.Patches.SignalPatches
{
    [HarmonyPatch(typeof(TravelerAudioManager))]
    public static class TravelerAudioManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TravelerAudioManager.Update))]
        public static void TravelerAudioManager_Update(TravelerAudioManager __instance)
        {
            __instance._signals.RemoveAll(signal => signal == null || signal.gameObject == null || signal._owAudioSource == null || signal._owAudioSource._audioSource == null);
        }
    }
}
