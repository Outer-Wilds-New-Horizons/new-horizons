using HarmonyLib;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.Utility.OWML;

namespace NewHorizons.Patches.SignalPatches
{
    [HarmonyPatch(typeof(Signalscope))]
    public static class SignalScopePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Signalscope.Awake))]
        public static void Signalscope_Awake(Signalscope __instance)
        {
            __instance._strongestSignals = new AudioSignal[32];
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Signalscope.SwitchFrequencyFilter))]
        public static bool Signalscope_SwitchFrequencyFilter(Signalscope __instance, int increment)
        {
            var count = SignalBuilder.NumberOfFrequencies;
            __instance._frequencyFilterIndex += increment;
            // Base game does 1 here but we use frequency index 0 as "default" or "???"
            __instance._frequencyFilterIndex = __instance._frequencyFilterIndex >= count ? 0 : __instance._frequencyFilterIndex;
            __instance._frequencyFilterIndex = __instance._frequencyFilterIndex < 0 ? count - 1 : __instance._frequencyFilterIndex;
            var signalFrequency = AudioSignal.IndexToFrequency(__instance._frequencyFilterIndex);

            NHLogger.Log($"Changed freq to {signalFrequency} at {__instance._frequencyFilterIndex}");

            // Skip over this frequency
            // Never skip traveler (always known)
            var isTraveler = __instance._frequencyFilterIndex == 1;
            var isUnknown = !PlayerData.KnowsFrequency(signalFrequency) && (!__instance._isUnknownFreqNearby || __instance._unknownFrequency != signalFrequency);
            if (!isTraveler && (isUnknown || !SignalBuilder.IsFrequencyInUse(signalFrequency)))
            {
                __instance.SwitchFrequencyFilter(increment);
            }

            return false;
        }
    }
}
