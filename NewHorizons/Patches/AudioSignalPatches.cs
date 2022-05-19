using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class AudioSignalPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.SignalNameToString))]
        public static bool AudioSignal_SignalNameToString(SignalName __0, ref string __result)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(__0);
            if (customSignalName == null) return true;
            else
            {
                __result = TranslationHandler.GetTranslation(customSignalName, TranslationHandler.TextType.UI).ToUpper();
                return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.FrequencyToIndex))]
        public static bool AudioSignal_FrequencyToIndex(SignalFrequency __0, ref int __result)
        {
            switch (__0)
            {
                case (SignalFrequency.Default):
                    __result = 0;
                    break;
                case (SignalFrequency.Traveler):
                    __result = 1;
                    break;
                case (SignalFrequency.Quantum):
                    __result = 2;
                    break;
                case (SignalFrequency.EscapePod):
                    __result = 3;
                    break;
                case (SignalFrequency.WarpCore):
                    __result = 4;
                    break;
                case (SignalFrequency.HideAndSeek):
                    __result = 5;
                    break;
                case (SignalFrequency.Radio):
                    __result = 6;
                    break;
                case (SignalFrequency.Statue):
                    __result = 7;
                    break;
                default:
                    // Frequencies are in powers of 2
                    __result = (int)(Mathf.Log((float)__0) / Mathf.Log(2f));
                    break;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.IndexToFrequency))]
        public static bool AudioSignal_IndexToFrequency(int __0, ref SignalFrequency __result)
        {
            switch (__0)
            {
                case 0:
                    __result = SignalFrequency.Default;
                    break;
                case 1:
                    __result = SignalFrequency.Traveler;
                    break;
                case 2:
                    __result = SignalFrequency.Quantum;
                    break;
                case 3:
                    __result = SignalFrequency.EscapePod;
                    break;
                case 4:
                    __result = SignalFrequency.WarpCore;
                    break;
                case 5:
                    __result = SignalFrequency.HideAndSeek;
                    break;
                case 6:
                    __result = SignalFrequency.Radio;
                    break;
                case 7:
                    __result = SignalFrequency.Statue;
                    break;
                default:
                    __result = (SignalFrequency)(Math.Pow(2, __0));
                    break;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.FrequencyToString))]
        public static bool AudioSignal_FrequencyToString(SignalFrequency __0, ref string __result)
        {
            var customName = SignalBuilder.GetCustomFrequencyName(__0);
            if (customName != null && customName != "")
            {
                if (NewHorizonsData.KnowsFrequency(customName)) __result = TranslationHandler.GetTranslation(customName, TranslationHandler.TextType.UI).ToUpper();
                else __result = UITextLibrary.GetString(UITextType.SignalFreqUnidentified);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioSignal), nameof(AudioSignal.UpdateSignalStrength))]
        public static bool AudioSignal_UpdateSignalStrength(AudioSignal __instance, Signalscope __0, float __1)
        {
            // I hate this, just because I can't override the base method in CloakedAudioSignal
            if (__instance is CloakedAudioSignal)
            {
                ((CloakedAudioSignal)__instance).UpdateSignalStrength(__0, __1);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TravelerAudioManager), nameof(TravelerAudioManager.Update))]
        public static void TravelerAudioManager_Update(TravelerAudioManager __instance)
        {
            __instance._signals.RemoveAll(signal => signal == null || signal.gameObject == null || signal._owAudioSource == null || signal._owAudioSource._audioSource == null);
        }
    }
}
