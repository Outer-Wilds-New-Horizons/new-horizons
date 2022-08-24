using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Handlers;
using System;
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
        public static bool AudioSignal_UpdateSignalStrength(AudioSignal __instance, Signalscope scope, float distToClosestScopeObstruction)
        {
            if (!SignalBuilder.Initialized) return true;

            if (!SignalBuilder.CloakedSignals.Contains(__instance._name) && !SignalBuilder.QMSignals.Contains(__instance._name)) return true;

            __instance._canBePickedUpByScope = false;
            if (__instance._sunController != null && __instance._sunController.IsPointInsideSupernova(__instance.transform.position))
            {
                __instance._signalStrength = 0f;
                __instance._degreesFromScope = 180f;
                return false;
            }

            // This part is modified from the original to include all QM signals
            if (Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsPlayerInside() && !SignalBuilder.QMSignals.Contains(__instance._name))
            {
                __instance._signalStrength = 0f;
                __instance._degreesFromScope = 180f;
                return false;
            }
            if (!__instance._active || !__instance.gameObject.activeInHierarchy || __instance._outerFogWarpVolume != PlayerState.GetOuterFogWarpVolume() || (scope.GetFrequencyFilter() & __instance._frequency) != __instance._frequency)
            {
                __instance._signalStrength = 0f;
                __instance._degreesFromScope = 180f;
                return false;
            }

            __instance._scopeToSignal = __instance.transform.position - scope.transform.position;
            __instance._distToScope = __instance._scopeToSignal.magnitude;
            if (__instance._outerFogWarpVolume == null && distToClosestScopeObstruction < 1000f && __instance._distToScope > 1000f)
            {
                __instance._signalStrength = 0f;
                __instance._degreesFromScope = 180f;
                return false;
            }
            __instance._canBePickedUpByScope = true;
            if (__instance._distToScope < __instance._sourceRadius)
            {
                __instance._signalStrength = 1f;
            }
            else
            {
                __instance._degreesFromScope = Vector3.Angle(scope.GetScopeDirection(), __instance._scopeToSignal);
                float t = Mathf.InverseLerp(2000f, 1000f, __instance._distToScope);
                float a = Mathf.Lerp(45f, 90f, t);
                float a2 = 57.29578f * Mathf.Atan2(__instance._sourceRadius, __instance._distToScope);
                float b = Mathf.Lerp(Mathf.Max(a2, 5f), Mathf.Max(a2, 1f), scope.GetZoomFraction());
                __instance._signalStrength = Mathf.Clamp01(Mathf.InverseLerp(a, b, __instance._degreesFromScope));
            }

            // If it's a cloaked signal we don't want to hear it outside the cloak field
            if (SignalBuilder.CloakedSignals.Contains(__instance._name))
            {
                if (!PlayerState.InCloakingField())
                {
                    __instance._signalStrength = 0f;
                    __instance._degreesFromScope = 180f;
                    return false;
                }
            }
            else
            {
                // This part is taken from the regular method
                if (Locator.GetCloakFieldController() != null)
                {
                    float num = 1f - Locator.GetCloakFieldController().playerCloakFactor;
                    __instance._signalStrength *= num;
                    if (OWMath.ApproxEquals(num, 0f, 0.001f))
                    {
                        __instance._signalStrength = 0f;
                        __instance._degreesFromScope = 180f;
                        return false;
                    }
                }
            }

            if (__instance._distToScope < __instance._identificationDistance + __instance._sourceRadius && __instance._signalStrength > 0.9f)
            {
                if (!PlayerData.KnowsFrequency(__instance._frequency) && !__instance._preventIdentification)
                {
                    __instance.IdentifyFrequency();
                }
                if (!PlayerData.KnowsSignal(__instance._name) && !__instance._preventIdentification)
                {
                    __instance.IdentifySignal();
                }
                if (__instance._revealFactID.Length > 0)
                {
                    Locator.GetShipLogManager().RevealFact(__instance._revealFactID, true, true);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TravelerAudioManager), nameof(TravelerAudioManager.Update))]
        public static void TravelerAudioManager_Update(TravelerAudioManager __instance)
        {
            __instance._signals.RemoveAll(signal => signal == null || signal.gameObject == null || signal._owAudioSource == null || signal._owAudioSource._audioSource == null);
        }
    }
}
