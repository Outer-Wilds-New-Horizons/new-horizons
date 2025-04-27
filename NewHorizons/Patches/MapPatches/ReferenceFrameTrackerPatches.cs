using HarmonyLib;
using NewHorizons.Components.Quantum;
using NewHorizons.Utility;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(ReferenceFrameTracker))]
    public static class ReferenceFrameTrackerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ReferenceFrameTracker.Awake))]
        public static void ReferenceFrameTracker_Awake(ReferenceFrameTracker __instance)
        {
            GlobalMessenger<QuantumPlanet>.AddListener("QuantumPlanetChangeOrbit", __instance.OnQuantumPlanetChangeOrbit);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ReferenceFrameTracker.OnDestroy))]
        public static void ReferenceFrameTracker_OnDestroy(ReferenceFrameTracker __instance)
        {
            GlobalMessenger<QuantumPlanet>.RemoveListener("QuantumPlanetChangeOrbit", __instance.OnQuantumPlanetChangeOrbit);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ReferenceFrameTracker.UntargetReferenceFrame), new System.Type[] { typeof(bool) })]
        public static bool ReferenceFrameTracker_UntargetReferenceFrame(ReferenceFrameTracker __instance, bool playAudio)
        {
            return __instance != null && __instance._hasTarget && __instance._currentReferenceFrame != null;
        }
    }
}
