using HarmonyLib;
using NewHorizons.Builder.Props.Audio;
using UnityEngine;

namespace NewHorizons.Patches.SignalPatches
{
    [HarmonyPatch(typeof(Signalscope))]
    public static class SignalScopePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Signalscope.Awake))]
        public static void Signalscope_Awake(Signalscope __instance)
        {
            __instance._strongestSignals = new AudioSignal[8];
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Signalscope.SwitchFrequencyFilter))]
        public static bool Signalscope_SwitchFrequencyFilter(Signalscope __instance, int increment)
        {
            var count = SignalBuilder.NumberOfFrequencies;
            __instance._frequencyFilterIndex = (__instance._frequencyFilterIndex + increment + count) % count;

            var signalFrequency = AudioSignal.IndexToFrequency(__instance._frequencyFilterIndex);

            var isUnknownNearby = __instance._isUnknownFreqNearby && __instance._unknownFrequency == signalFrequency;

            // If it's not in use, or its unknown and isn't about to be learned, then go to the next one.
            if ((!PlayerData.KnowsFrequency(signalFrequency) && !isUnknownNearby) || !SignalBuilder.IsFrequencyInUse(signalFrequency)) 
            {
                __instance.SwitchFrequencyFilter(increment);
            }

            return false;
        }
    }
}
