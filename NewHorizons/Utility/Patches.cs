using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class Patches
    {
        public static void Apply()
        {
            // Prefixes
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ReferenceFrame>("GetHUDDisplayName", typeof(Patches), nameof(Patches.GetHUDDisplayName));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<PlayerState>("CheckShipOutsideSolarSystem", typeof(Patches), nameof(Patches.CheckShipOutersideSolarSystem));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SunLightParamUpdater>("LateUpdate", typeof(Patches), nameof(Patches.OnSunLightParamUpdaterLateUpdate));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SunSurfaceAudioController>("Update", typeof(Patches), nameof(Patches.OnSunSurfaceAudioControllerUpdate));

            // Lot of audio signal stuff
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<AudioSignal>("SignalNameToString", typeof(Patches), nameof(Patches.OnAudioSignalSignalNameToString));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<AudioSignal>("IndexToFrequency", typeof(Patches), nameof(Patches.OnAudioSignalIndexToFrequency));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<AudioSignal>("FrequencyToIndex", typeof(Patches), nameof(Patches.OnAudioSignalFrequencyToIndex));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<AudioSignal>("FrequencyToString", typeof(Patches), nameof(Patches.OnAudioSignalFrequencyToString));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<Signalscope>("Awake", typeof(Patches), nameof(Patches.OnSignalscopeAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<Signalscope>("SwitchFrequencyFilter", typeof(Patches), nameof(Patches.OnSignalscopeSwitchFrequencyFilter));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<AudioSignal>("UpdateSignalStrength", typeof(Patches), nameof(Patches.OnAudioSignalUpdateSignalStrength));

            var playerDataKnowsSignal = typeof(PlayerData).GetMethod("KnowsSignal");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataKnowsSignal, typeof(Patches), nameof(Patches.OnPlayerDataKnowsSignal));
            var playerDataLearnSignal = typeof(PlayerData).GetMethod("LearnSignal");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataLearnSignal, typeof(Patches), nameof(Patches.OnPlayerDataLearnSignal));
            var playerDataKnowsFrequency = typeof(PlayerData).GetMethod("KnowsFrequency");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataKnowsFrequency, typeof(Patches), nameof(Patches.OnPlayerDataKnowsFrequency));
            var playerDataLearnFrequency = typeof(PlayerData).GetMethod("LearnFrequency");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataLearnFrequency, typeof(Patches), nameof(Patches.OnPlayerDataLearnFrequency));
            var playerDataKnowsMultipleFrequencies = typeof(PlayerData).GetMethod("KnowsMultipleFrequencies");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataKnowsMultipleFrequencies, typeof(Patches), nameof(Patches.OnPlayerDataKnowsMultipleFrequencies));  
            var playerDataResetGame = typeof(PlayerData).GetMethod("ResetGame");
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix(playerDataResetGame, typeof(Patches), nameof(Patches.OnPlayerDataResetGame));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<BlackHoleVolume>("Start", typeof(Patches), nameof(Patches.OnBlackHoleVolumeStart));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<WhiteHoleVolume>("Awake", typeof(Patches), nameof(Patches.OnWhiteHoleVolumeAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ProbeLauncher>("UpdateOrbitalLaunchValues", typeof(Patches), nameof(Patches.OnProbeLauncherUpdateOrbitalLaunchValues));

            // Postfixes
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<MapController>("Awake", typeof(Patches), nameof(Patches.OnMapControllerAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<OWCamera>("Awake", typeof(Patches), nameof(Patches.OnOWCameraAwake));
        }

        public static bool GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();
            if (ao != null && ao.GetAstroObjectName() == AstroObject.Name.CustomString)
            {
                __result = ao.GetCustomName();
                return false;
            }
            return true;
        }

        public static bool CheckShipOutersideSolarSystem(PlayerState __instance, ref bool __result)
        {
            __result = false;
            return false;
        }

        public static void OnMapControllerAwake(MapController __instance, ref float ____maxPanDistance, ref float ____maxZoomDistance, ref float ____minPitchAngle, ref float ____zoomSpeed)
        {
            ____maxPanDistance = Main.FurthestOrbit * 1.5f;
            ____maxZoomDistance *= 6f;
            ____minPitchAngle = -90f;
            ____zoomSpeed *= 4f;
            __instance._mapCamera.farClipPlane = Main.FurthestOrbit * 3f;
        }

        public static void OnOWCameraAwake(OWCamera __instance)
        {
            __instance.farClipPlane *= 4f;
        }

        public static bool OnSunLightParamUpdaterLateUpdate(SunLightParamUpdater __instance)
        {
            if (__instance.sunLight)
            {
                Vector3 position = __instance.transform.position;
                float w = 2000f;
                if (__instance._sunController != null)
                {
                    w = (__instance._sunController.HasSupernovaStarted() ? __instance._sunController.GetSupernovaRadius() : __instance._sunController.GetSurfaceRadius());
                }
                float range = __instance.sunLight.range;
                Color color = (__instance._sunLightController != null) ? __instance._sunLightController.sunColor : __instance.sunLight.color;
                float w2 = (__instance._sunLightController != null) ? __instance._sunLightController.sunIntensity : __instance.sunLight.intensity;
                Shader.SetGlobalVector(__instance._propID_SunPosition, new Vector4(position.x, position.y, position.z, w));
                Shader.SetGlobalVector(__instance._propID_OWSunPositionRange, new Vector4(position.x, position.y, position.z, 1f / (range * range)));
                Shader.SetGlobalVector(__instance._propID_OWSunColorIntensity, new Vector4(color.r, color.g, color.b, w2));
            }

            return false;
        }

        public static bool OnSunSurfaceAudioControllerUpdate(SunSurfaceAudioController __instance)
        {
            if (__instance._sunController != null) return true;

            var surfaceRadius = __instance.transform.parent.parent.localScale.magnitude;
            float value = Mathf.Max(0f, Vector3.Distance(Locator.GetPlayerCamera().transform.position, __instance.transform.position) - surfaceRadius);
            float num = Mathf.InverseLerp(1600f, 100f, value);
            __instance._audioSource.SetLocalVolume(num * num * __instance._fade);
            return false;
        }

        #region AudioSignal

        public static bool OnAudioSignalSignalNameToString(SignalName __0, ref string __result)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(__0);
            if (customSignalName == null) return true;
            else
            {
                __result = customSignalName;
                return false;
            }
        }

        public static bool OnAudioSignalIndexToFrequency(int __0, ref SignalFrequency __result) {
            switch (__0)
            {
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
                    __result = SignalFrequency.Default;
                    break;
            }
            return false;
        }

        public static bool OnAudioSignalFrequencyToIndex(SignalFrequency __0, ref int __result)
        {
            var frequency = __0;
            if (frequency <= SignalFrequency.EscapePod)
            {
                if(frequency == SignalFrequency.Default)
                {
                    __result = 0;
                }
                else if (frequency == SignalFrequency.Traveler)
                {
                    __result = 1;
                }
                else if (frequency == SignalFrequency.Quantum)
                {
                    __result = 2;
                }
                else if (frequency == SignalFrequency.EscapePod)
                {
                    __result = 3;
                }
            }
            else
            {
                if (frequency == SignalFrequency.WarpCore)
                {
                    __result = 4;
                }
                else if (frequency == SignalFrequency.HideAndSeek)
                {
                    __result = 5;
                }
                else if (frequency == SignalFrequency.Radio)
                {
                    __result = 6;
                }
                else if (frequency == SignalFrequency.Statue)
                {
                    __result = 7;
                }
            }
            return false;
        }

        public static bool OnAudioSignalFrequencyToString(SignalFrequency __0, ref string __result)
        {
            SignalBuilder.SignalFrequencyOverrides.TryGetValue(__0, out string customName);
            if (customName != null) 
            {
                if (NewHorizonsData.KnowsFrequency(customName)) __result = customName;
                else __result = UITextLibrary.GetString(UITextType.SignalFreqUnidentified);
                return false;
            }
            return true;
        }

        public static bool OnAudioSignalUpdateSignalStrength(AudioSignal __instance, Signalscope __0, float __1)
        {             
            // I hate this
            if(__instance is CloakedAudioSignal)
            {
                ((CloakedAudioSignal)__instance).UpdateSignalStrength(__0, __1);
                return false;
            }
            return true;
        }
        #endregion

        #region Signalscope
        public static bool OnSignalscopeAwake(Signalscope __instance, ref AudioSignal[] ____strongestSignals)
        {
            ____strongestSignals = new AudioSignal[8];
            return true;
        }

        public static bool OnSignalscopeSwitchFrequencyFilter(Signalscope __instance, int __0)
        {
            var increment = __0;
            var count = Enum.GetValues(typeof(SignalFrequency)).Length;
            __instance._frequencyFilterIndex += increment;
            __instance._frequencyFilterIndex = ((__instance._frequencyFilterIndex >= count) ? 0 : __instance._frequencyFilterIndex);
            __instance._frequencyFilterIndex = ((__instance._frequencyFilterIndex < 0) ? count - 1 : __instance._frequencyFilterIndex);
            SignalFrequency signalFrequency = AudioSignal.IndexToFrequency(__instance._frequencyFilterIndex);
            if (!PlayerData.KnowsFrequency(signalFrequency) && (!__instance._isUnknownFreqNearby || __instance._unknownFrequency != signalFrequency))
            {
                __instance.SwitchFrequencyFilter(increment);
            }
            return false;
        }
        #endregion

        #region PlayerData
        public static bool OnPlayerDataKnowsFrequency(SignalFrequency __0, ref bool __result)
        {
            SignalBuilder.SignalFrequencyOverrides.TryGetValue(__0, out string freqString);
            if (freqString != null)
            {
                __result = NewHorizonsData.KnowsFrequency(freqString);
                return false;
            }
            return true;
        }

        public static bool OnPlayerDataLearnFrequency(SignalFrequency __0)
        {
            SignalBuilder.SignalFrequencyOverrides.TryGetValue(__0, out string freqString);
            if (freqString != null)
            {
                NewHorizonsData.LearnFrequency(freqString);
                return false;
            }
            return true;
        }

        public static bool OnPlayerDataKnowsSignal(SignalName __0, ref bool __result)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(__0);
            if (customSignalName != null)
            {
                __result = NewHorizonsData.KnowsSignal(customSignalName);
                return false;
            }
            return true;
        }

        public static bool OnPlayerDataLearnSignal(SignalName __0)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(__0);
            if (customSignalName != null)
            {
                if (!NewHorizonsData.KnowsSignal(customSignalName)) NewHorizonsData.LearnSignal(customSignalName);
                return false;
            }
            return true;
        }

        public static bool OnPlayerDataKnowsMultipleFrequencies(ref bool __result)
        {
            if (NewHorizonsData.KnowsMultipleFrequencies())
            {
                __result = true;
                return false;
            }
            return true;
        }

        public static void OnPlayerDataResetGame()
        {
            NewHorizonsData.Reset();
        }
        #endregion

        public static bool OnBlackHoleVolumeStart(BlackHoleVolume __instance)
        {
            return __instance._whiteHole == null;
        }

        public static bool OnWhiteHoleVolumeAwake(WhiteHoleVolume __instance)
        {
            __instance._growQueue = new List<OWRigidbody>(8);
            __instance._growQueueLocationData = new List<RelativeLocationData>(8);
            __instance._ejectedBodyList = new List<OWRigidbody>(64);
            try
            {
                __instance._whiteHoleBody = __instance.gameObject.GetAttachedOWRigidbody(false);
                __instance._whiteHoleProxyShadowSuperGroup = __instance._whiteHoleBody.GetComponentInChildren<ProxyShadowCasterSuperGroup>();
                __instance._fluidVolume = __instance.gameObject.GetRequiredComponent<WhiteHoleFluidVolume>();
            }
            catch (Exception) { }
            return false;
        }

        public static bool OnProbeLauncherUpdateOrbitalLaunchValues(ProbeLauncher __instance)
        {
            return (Locator.GetPlayerRulesetDetector()?.GetPlanetoidRuleset()?.GetGravityVolume() != null);
        }
    }
}
