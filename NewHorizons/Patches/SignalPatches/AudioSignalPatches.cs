using HarmonyLib;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using UnityEngine;

namespace NewHorizons.Patches.SignalPatches
{
    [HarmonyPatch(typeof(AudioSignal))]
    public static class AudioSignalPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(AudioSignal.IsActive))]
        public static void AudioSignal_IsActive(AudioSignal __instance, ref bool __result)
        {
            __result = __result && __instance.gameObject.activeInHierarchy;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AudioSignal.SignalNameToString))]
        public static bool AudioSignal_SignalNameToString(SignalName name, ref string __result)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(name);
            if (!string.IsNullOrEmpty(customSignalName))
            {
                __result = TranslationHandler.GetTranslation(customSignalName, TranslationHandler.TextType.UI, false).ToUpperFixed();
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AudioSignal.FrequencyToIndex))]
        public static bool AudioSignal_FrequencyToIndex(SignalFrequency frequency, out int __result)
        {
            __result = frequency switch
            {
                SignalFrequency.Default => 0,
                SignalFrequency.Traveler => 1,
                SignalFrequency.Quantum => 2,
                SignalFrequency.EscapePod => 3,
                SignalFrequency.WarpCore => 4,
                SignalFrequency.HideAndSeek => 5,
                SignalFrequency.Radio => 6,
                SignalFrequency.Statue => 7,
                _ => (int)(Mathf.Log((float)frequency) / Mathf.Log(2f)),// Frequencies are in powers of 2
            };
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AudioSignal.IndexToFrequency))]
        public static bool AudioSignal_IndexToFrequency(int index, out SignalFrequency __result)
        {
            __result = index switch
            {
                0 => SignalFrequency.Default,
                1 => SignalFrequency.Traveler,
                2 => SignalFrequency.Quantum,
                3 => SignalFrequency.EscapePod,
                4 => SignalFrequency.WarpCore,
                5 => SignalFrequency.HideAndSeek,
                6 => SignalFrequency.Radio,
                7 => SignalFrequency.Statue,
                _ => (SignalFrequency)Math.Pow(2, index),
            };
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AudioSignal.FrequencyToString))]
        public static bool AudioSignal_FrequencyToString(SignalFrequency frequency, ref string __result)
        {
            var customName = SignalBuilder.GetCustomFrequencyName(frequency);
            if (!string.IsNullOrEmpty(customName))
            {
                if (NewHorizonsData.KnowsFrequency(customName)) __result = TranslationHandler.GetTranslation(customName, TranslationHandler.TextType.UI, false).ToUpperFixed();
                else __result = UITextLibrary.GetString(UITextType.SignalFreqUnidentified);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(AudioSignal.UpdateSignalStrength))]
        public static bool AudioSignal_UpdateSignalStrength(AudioSignal __instance, Signalscope scope, float distToClosestScopeObstruction)
        {
            if (!SignalBuilder.Initialized) return true;

            var isCloaked = __instance.IsCloaked();
            var isOnQuantumMoon = __instance.IsOnQuantumMoon();

            if (!isCloaked && !isOnQuantumMoon) return true;

            __instance._canBePickedUpByScope = false;
            if (__instance._sunController != null && __instance._sunController.IsPointInsideSupernova(__instance.transform.position))
            {
                __instance._signalStrength = 0f;
                __instance._degreesFromScope = 180f;
                return false;
            }

            // This part is modified from the original to include all QM signals
            if (Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsPlayerInside() && !isOnQuantumMoon)
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
            if (isCloaked)
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
    }
}
