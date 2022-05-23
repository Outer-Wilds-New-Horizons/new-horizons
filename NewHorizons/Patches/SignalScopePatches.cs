using HarmonyLib;
using NewHorizons.Builder.Props;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class SignalScopePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.Awake))]
        public static bool Signalscope_Awake(Signalscope __instance, ref AudioSignal[] ____strongestSignals)
        {
            ____strongestSignals = new AudioSignal[8];
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Signalscope), nameof(Signalscope.SwitchFrequencyFilter))]
        public static bool Signalscope_SwitchFrequencyFilter(Signalscope __instance, int __0)
        {
            var increment = __0;
            var count = SignalBuilder.NumberOfFrequencies;
            __instance._frequencyFilterIndex += increment;
            __instance._frequencyFilterIndex =
                __instance._frequencyFilterIndex >= count ? 0 : __instance._frequencyFilterIndex;
            __instance._frequencyFilterIndex =
                __instance._frequencyFilterIndex < 0 ? count - 1 : __instance._frequencyFilterIndex;
            var signalFrequency = AudioSignal.IndexToFrequency(__instance._frequencyFilterIndex);

            if (!PlayerData.KnowsFrequency(signalFrequency) &&
                (!__instance._isUnknownFreqNearby || __instance._unknownFrequency != signalFrequency))
                __instance.SwitchFrequencyFilter(increment);
            return false;
        }
    }
}