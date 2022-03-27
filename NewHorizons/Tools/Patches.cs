using NewHorizons.Builder.General;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Harmony;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;
using NewHorizons.Handlers;
using NewHorizons.Builder.ShipLog;

namespace NewHorizons.Tools
{
    public class Patches
    {
        public static void Apply()
        {
            // Prefixes
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<PlayerState>("CheckShipOutsideSolarSystem", typeof(Patches), nameof(Patches.CheckShipOutersideSolarSystem));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SunLightParamUpdater>("LateUpdate", typeof(Patches), nameof(Patches.OnSunLightParamUpdaterLateUpdate));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SunSurfaceAudioController>("Update", typeof(Patches), nameof(Patches.OnSunSurfaceAudioControllerUpdate));

            var locatorRegisterCloakFieldController = typeof(Locator).GetMethod(nameof(Locator.RegisterCloakFieldController));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(locatorRegisterCloakFieldController, typeof(Patches), nameof(Patches.OnLocatorRegisterCloakFieldController));

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

            var playerDataAddNewlyRevealedFactID = typeof(PlayerData).GetMethod("AddNewlyRevealedFactID");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataAddNewlyRevealedFactID, typeof(Patches), nameof(Patches.OnPlayerDataAddNewlyRevealedFactID));
            var playerDataGetNewlyRevealedFactIDs = typeof(PlayerData).GetMethod("GetNewlyRevealedFactIDs");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataGetNewlyRevealedFactIDs, typeof(Patches), nameof(Patches.OnPlayerDataGetNewlyRevealedFactIDs));
            var playerDataClearNewlyRevealedFactIDs = typeof(PlayerData).GetMethod("ClearNewlyRevealedFactIDs");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataClearNewlyRevealedFactIDs, typeof(Patches), nameof(Patches.OnPlayerDataClearNewlyRevealedFactIDs));
            

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<BlackHoleVolume>("Start", typeof(Patches), nameof(Patches.OnBlackHoleVolumeStart));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<WhiteHoleVolume>("Awake", typeof(Patches), nameof(Patches.OnWhiteHoleVolumeAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ProbeLauncher>("UpdateOrbitalLaunchValues", typeof(Patches), nameof(Patches.OnProbeLauncherUpdateOrbitalLaunchValues));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SurveyorProbe>("IsLaunched", typeof(Patches), nameof(Patches.OnSurveyorProbeIsLaunched));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<PlayerSpawner>("SpawnPlayer", typeof(Patches), nameof(Patches.OnPlayerSpawnerSpawnPlayerPreFix));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(Patches), nameof(Patches.OnDeathManagerKillPlayer));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipThrusterController>("ReadTranslationalInput", typeof(Patches), nameof(Patches.OnShipThrusterControllerReadTranslationalInput));

            // Postfixes
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<MapController>("Awake", typeof(Patches), nameof(Patches.OnMapControllerAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<MapController>("OnTargetReferenceFrame", typeof(Patches), nameof(Patches.OnMapControllerOnTargetReferenceFrame));
        }

        public static bool CheckShipOutersideSolarSystem(PlayerState __instance, ref bool __result)
        {
            if (PlayerState._inBrambleDimension) return false;

            Transform sunTransform = Locator.GetSunTransform();
            OWRigidbody shipBody = Locator.GetShipBody();
            var maxDist2 = Mathf.Max(900000000f, Main.FurthestOrbit * Main.FurthestOrbit * 2f);
            __result = sunTransform != null && shipBody != null && (sunTransform.position - shipBody.transform.position).sqrMagnitude > maxDist2;
            return false;
        }

        public static void OnMapControllerAwake(MapController __instance, ref float ____maxPanDistance, ref float ____maxZoomDistance, ref float ____minPitchAngle, ref float ____zoomSpeed)
        {
            ____maxPanDistance = Main.FurthestOrbit * 1.5f;
            ____maxZoomDistance *= 6f;
            ____minPitchAngle = -90f;
            ____zoomSpeed *= 4f;
            __instance._mapCamera.farClipPlane = Main.FurthestOrbit * 10f;
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
                __result = TranslationHandler.GetTranslation(customSignalName, TranslationHandler.TextType.UI).ToUpper();
                return false;
            }
        }

        public static bool OnAudioSignalFrequencyToIndex(SignalFrequency __0, ref int __result)
        {
            switch(__0)
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

        public static bool OnAudioSignalIndexToFrequency(int __0, ref SignalFrequency __result) {
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

        public static bool OnAudioSignalFrequencyToString(SignalFrequency __0, ref string __result)
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
            var count = SignalBuilder.NumberOfFrequencies;
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

        #endregion                                                                                    f

        #region PlayerData
        public static bool OnPlayerDataKnowsFrequency(SignalFrequency __0, ref bool __result)
        {
            var freqString = SignalBuilder.GetCustomFrequencyName(__0);

            if (freqString != null && freqString != "")
            { 
                __result = NewHorizonsData.KnowsFrequency(freqString);
                return false;
            }
            return true;
        }

        public static bool OnPlayerDataLearnFrequency(SignalFrequency __0)
        {
            var freqString = SignalBuilder.GetCustomFrequencyName(__0);
            if (freqString != null && freqString != "")
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

        public static bool OnPlayerDataAddNewlyRevealedFactID(string __0)
        {
            if (ShipLogHandler.IsModdedFact(__0))
            {
                NewHorizonsData.AddNewlyRevealedFactID(__0);
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool OnPlayerDataGetNewlyRevealedFactIDs(ref List<string> __result)
        {
            __result = PlayerData._currentGameSave.newlyRevealedFactIDs.Concat(NewHorizonsData.GetNewlyRevealedFactIDs()).ToList();
            return false;
        }

        public static bool OnPlayerDataClearNewlyRevealedFactIDs()
        {
            PlayerData._currentGameSave.newlyRevealedFactIDs.Clear();
            NewHorizonsData.ClearNewlyRevealedFactIDs();
            return false;
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

        public static bool OnSurveyorProbeIsLaunched(SurveyorProbe __instance, ref bool __result)
        {
            try
            {
                __result = __instance.gameObject.activeSelf;
            }
            catch(Exception)
            {
                __result = true;
            }
            return false;
        }

        public static void OnMapControllerOnTargetReferenceFrame(MapController __instance, ReferenceFrame __0)
        {
            __instance._isLockedOntoMapSatellite = true;
        }


        public static void OnPlayerSpawnerSpawnPlayerPreFix(PlayerSpawner __instance)
        {
            Logger.Log("Player spawning");
            __instance.SetInitialSpawnPoint(Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint);
        }

        public static bool OnDeathManagerKillPlayer()
        {
            return (Main.Instance.CurrentStarSystem != "EyeOfTheUniverse");
        }

        public static bool OnShipThrusterControllerReadTranslationalInput(ShipThrusterController __instance, ref Vector3 __result)
        {
            if (Main.Instance.CurrentStarSystem != "EyeOfTheUniverse") return true;

            float value = OWInput.GetValue(InputLibrary.thrustX, InputMode.All);
            float value2 = OWInput.GetValue(InputLibrary.thrustZ, InputMode.All);
            float value3 = OWInput.GetValue(InputLibrary.thrustUp, InputMode.All);
            float value4 = OWInput.GetValue(InputLibrary.thrustDown, InputMode.All);
            if (!OWInput.IsInputMode(InputMode.ShipCockpit | InputMode.LandingCam))
            {
                __result = Vector3.zero;
                return false;
            }
            if (!__instance._shipResources.AreThrustersUsable())
            {
                __result = Vector3.zero;
                return false;
            }
            if (__instance._autopilot.IsFlyingToDestination())
            {
                __result = Vector3.zero;
                return false;
            }
            Vector3 vector = new Vector3(value, 0f, value2);
            if (vector.sqrMagnitude > 1f)
            {
                vector.Normalize();
            }
            vector.y = value3 - value4;
            if (__instance._requireIgnition && __instance._landingManager.IsLanded())
            {
                vector.x = 0f;
                vector.z = 0f;
                vector.y = Mathf.Clamp01(vector.y);
                if (!__instance._isIgniting && __instance._lastTranslationalInput.y <= 0f && vector.y > 0f)
                {
                    __instance._isIgniting = true;
                    __instance._ignitionTime = Time.time;
                    GlobalMessenger.FireEvent("StartShipIgnition");
                }
                if (__instance._isIgniting)
                {
                    if (vector.y <= 0f)
                    {
                        __instance._isIgniting = false;
                        GlobalMessenger.FireEvent("CancelShipIgnition");
                    }
                    if (Time.time < __instance._ignitionTime + __instance._ignitionDuration)
                    {
                        vector.y = 0f;
                    }
                    else
                    {
                        __instance._isIgniting = false;
                        __instance._requireIgnition = false;
                        GlobalMessenger.FireEvent("CompleteShipIgnition");
                        RumbleManager.PlayShipIgnition();
                    }
                }
            }
            float d = __instance._thrusterModel.GetMaxTranslationalThrust() / __instance._thrusterModel.GetMaxTranslationalThrust();
            Vector3 vector2 = vector * d;
            if (__instance._limitOrbitSpeed && vector2.magnitude > 0f)
            {
                Vector3 vector3 = __instance._landingRF.GetOWRigidBody().GetWorldCenterOfMass() - __instance._shipBody.GetWorldCenterOfMass();
                Vector3 vector4 = __instance._shipBody.GetVelocity() - __instance._landingRF.GetVelocity();
                Vector3 vector5 = vector4 - Vector3.Project(vector4, vector3);
                Vector3 vector6 = Quaternion.FromToRotation(-__instance._shipBody.transform.up, vector3) * __instance._shipBody.transform.TransformDirection(vector2 * __instance._thrusterModel.GetMaxTranslationalThrust());
                Vector3 vector7 = Vector3.Project(vector6, vector3);
                Vector3 vector8 = vector6 - vector7;
                Vector3 a = vector5 + vector8 * Time.deltaTime;
                float magnitude = a.magnitude;
                float orbitSpeed = __instance._landingRF.GetOrbitSpeed(vector3.magnitude);
                if (magnitude > orbitSpeed)
                {
                    a = a.normalized * orbitSpeed;
                    vector8 = (a - vector5) / Time.deltaTime;
                    vector6 = vector7 + vector8;
                    vector2 = __instance._shipBody.transform.InverseTransformDirection(vector6 / __instance._thrusterModel.GetMaxTranslationalThrust());
                }
            }
            __instance._lastTranslationalInput = vector;
            __result = vector2;

            return false;
        }

        public static bool OnLocatorRegisterCloakFieldController()
        {
            return Locator._cloakFieldController == null;
        }
    }
}
