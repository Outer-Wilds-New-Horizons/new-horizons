using HarmonyLib;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(ReferenceFrameTracker))]
    public static class ReferenceFrameTrackerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ReferenceFrameTracker.UntargetReferenceFrame), new System.Type[] { typeof(bool) })]
        public static bool ReferenceFrameTracker_UntargetReferenceFrame(ReferenceFrameTracker __instance, bool playAudio)
        {
            return __instance != null && __instance._hasTarget && __instance._currentReferenceFrame != null;
        }
    }
}
