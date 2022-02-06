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

namespace NewHorizons.Tools
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
            var playerDataGetNewlyRevealedFactIDs = typeof(PlayerData).GetMethod("GetNewlyRevealedFactIDs");
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix(playerDataGetNewlyRevealedFactIDs, typeof(Patches), nameof(Patches.OnPlayerDataGetNewlyRevealedFactIDsComplete));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<BlackHoleVolume>("Start", typeof(Patches), nameof(Patches.OnBlackHoleVolumeStart));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<WhiteHoleVolume>("Awake", typeof(Patches), nameof(Patches.OnWhiteHoleVolumeAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ProbeLauncher>("UpdateOrbitalLaunchValues", typeof(Patches), nameof(Patches.OnProbeLauncherUpdateOrbitalLaunchValues));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SurveyorProbe>("IsLaunched", typeof(Patches), nameof(Patches.OnSurveyorProbeIsLaunched));

            
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("Awake", typeof(Patches), nameof(Patches.OnShipLogManagerAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("Start", typeof(Patches), nameof(Patches.OnShipLogManagerStart));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("IsFactRevealed", typeof(Patches), nameof(Patches.OnShipLogManagerIsFactRevealed));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("CheckForCompletionAchievement", typeof(Patches), nameof(Patches.OnShipLogManagerCheckForCompletionAchievement));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<UIStyleManager>("GetCuriosityColor", typeof(Patches), nameof(Patches.OnUIStyleManagerGetCuriosityColor));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogSandFunnel>("Awake", typeof(Patches), nameof(Patches.DisableShipLogSandFunnel));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogSandFunnel>("UpdateState", typeof(Patches), nameof(Patches.DisableShipLogSandFunnel));
            
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogAstroObject>("GetName", typeof(Patches), nameof(Patches.OnShipLogAstroObjectGetName));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipCockpitController>("Update", typeof(Patches), nameof(Patches.OnShipCockpitControllerUpdate));

            // Postfixes
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<MapController>("Awake", typeof(Patches), nameof(Patches.OnMapControllerAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogMapMode>("EnterMode", typeof(Patches), nameof(Patches.OnShipLogMapModeEnterMode));
            
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogManager>("Awake", typeof(Patches), nameof(Patches.OnShipLogManagerAwakeComplete));

            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogMapMode>("Initialize", typeof(Patches), nameof(Patches.OnShipLogMapModeInitialize));
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
            Transform sunTransform = Locator.GetSunTransform();
            OWRigidbody shipBody = Locator.GetShipBody();
            __result = sunTransform != null && shipBody != null && (sunTransform.position - shipBody.transform.position).sqrMagnitude > Main.FurthestOrbit * Main.FurthestOrbit * 4f;
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

        public static bool OnShipCockpitControllerUpdate(ShipCockpitController __instance)
        {
            if(__instance._playerAtFlightConsole && OWInput.IsNewlyPressed(InputLibrary.autopilot, InputMode.ShipCockpit))
            {
                var targetSystem = ShipLogBuilder.ShipLogStarChartMode.GetTargetStarSystem();
                if (targetSystem != null)
                {
                    Main.Instance.ChangeCurrentStarSystem(targetSystem, true);
                    return false;
                }
            }
            return true;
        }
        
        #region ShipLog
        public static void OnShipLogManagerAwake(ShipLogManager __instance)
        {
            Logger.Log("Beginning Ship Log Generation For: " + Main.Instance.CurrentStarSystem, Logger.LogType.Log);
            if (Main.Instance.CurrentStarSystem != "SolarSystem")
            {
                __instance._shipLogXmlAssets = new TextAsset[] {};
                foreach (ShipLogEntryLocation logEntryLocation in GameObject.FindObjectsOfType<ShipLogEntryLocation>())
                {
                    logEntryLocation._initialized = true;
                }
            }
            foreach (NewHorizonsBody body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                if (body.Config.ShipLog?.curiosities != null)
                {
                    ShipLogBuilder.RumorModeBuilder.AddCuriosityColors(body.Config.ShipLog.curiosities);
                }
            }
            foreach (NewHorizonsBody body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                if (body.Config.ShipLog?.xmlFile != null)
                {
                    ShipLogBuilder.RumorModeBuilder.AddBodyToShipLog(__instance, body);
                }
            }
        }

        public static void OnShipLogManagerAwakeComplete(ShipLogManager __instance)
        {
            ShipLogBuilder.RumorModeBuilder.GenerateEntryData(__instance);
            for (var i = 0; i < __instance._entryList.Count; i++)
            {
                ShipLogEntry logEntry = __instance._entryList[i];
                ShipLogBuilder.RumorModeBuilder.UpdateEntryCuriosity(ref logEntry);
            }
            Logger.Log("Ship Log Generation Complete For: " + Main.Instance.CurrentStarSystem, Logger.LogType.Log);
        }

        public static bool OnShipLogManagerIsFactRevealed(ShipLogManager __instance, ref bool __result, string __0)
        {
            if (Main.Instance.CurrentStarSystem == "SolarSystem")
            {
                return true;
            }
            else
            {
                if (__instance._factDict.ContainsKey(__0) == false)
                {
                    __result = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static bool OnShipLogManagerCheckForCompletionAchievement()
        {
            return Main.Instance.CurrentStarSystem == "SolarSystem";
        }

        public static bool OnShipLogManagerStart(ShipLogManager __instance)
        {
            if (Main.Instance.CurrentStarSystem == "SolarSystem")
            {
                return true;
            }
            else
            {
                foreach (NewHorizonsBody body in Main.BodyDict[Main.Instance.CurrentStarSystem])
                {
                    foreach (string fact in body.Config.ShipLog?.initialReveal ?? Array.Empty<string>())
                    {
                        __instance.RevealFact(fact, false, false);
                    }
                }
                ShipLogBuilder.EntryLocationBuilder.InitializeLocations();
                return false;
            }
        }

        public static bool OnUIStyleManagerGetCuriosityColor(UIStyleManager __instance, CuriosityName __0, bool __1, ref Color __result)
        {
            if (Main.Instance.CurrentStarSystem == "SolarSystem")
            {
                return true;
            }
            else
            {
                __result = ShipLogBuilder.RumorModeBuilder.GetCuriosityColor(__0, __1, __instance._neutralColor, __instance._neutralHighlight);
                return false;
            }
        }

        private static void DeleteDetail(string name)
        {
            Object.Destroy(GameObject.Find(ShipLogBuilder.PAN_ROOT_PATH + "/" + name));
        }

        public static void OnShipLogMapModeInitialize(ShipLogMapMode __instance)
        {
            if (Main.Instance.CurrentStarSystem != "SolarSystem")
            {
                GameObject panRoot = GameObject.Find(ShipLogBuilder.PAN_ROOT_PATH);
                GameObject sunObject = GameObject.Find(ShipLogBuilder.PAN_ROOT_PATH + "/Sun");
                ShipLogAstroObject[][] navMatrix = ShipLogBuilder.MapModeBuilder.ConstructMapMode(Main.Instance.CurrentStarSystem, panRoot,  sunObject.layer);
                if (navMatrix.Length <= 1)
                {
                    Logger.LogWarning("No planets suitable for map mode found! Defaulting to vanilla menu (expect weirdness!).");
                }
                else
                {
                    __instance._astroObjects = navMatrix;
                    __instance._startingAstroObjectID = navMatrix[1][0].GetID();
                    List<GameObject> delete = SearchUtilities.GetAllChildren(panRoot).Where(g => g.name.Contains("_ShipLog") == false).ToList();
                    foreach (GameObject gameObject in delete)
                    {
                        DeleteDetail(gameObject.name);
                    }
                    // Just Lie About Having A Sand Funnel
                    __instance._sandFunnel = __instance.gameObject.AddComponent<ShipLogSandFunnel>();
                }
            }
            Logger.Log("Map Mode Construction Complete", Logger.LogType.Log);
        }

        public static bool OnShipLogAstroObjectGetName(ShipLogAstroObject __instance, ref string __result)
        {
            if (Main.Instance.CurrentStarSystem == "SolarSystem")
            {
                return true;
            }
            else
            {
                __result = ShipLogBuilder.MapModeBuilder.GetAstroBodyShipLogName(__instance.GetID());
                return false;
            }
        }

        public static bool DisableShipLogSandFunnel()
        {
            return Main.Instance.CurrentStarSystem == "SolarSystem";
        }

        public static void OnPlayerDataGetNewlyRevealedFactIDsComplete(ref List<string> __result)
        {
            ShipLogManager manager = Locator.GetShipLogManager();
            __result = __result.Where(e => manager.GetFact(e) != null).ToList();
        }
        # endregion

        public static void OnShipLogMapModeEnterMode(ShipLogMapMode __instance)
        {
            var newPrompt = "Interstellar Mode";
            __instance._detectiveModePrompt.SetText(newPrompt);
            var text = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/ScreenPromptListScaleRoot/ScreenPromptList_UpperRight/ScreenPrompt/Text").GetComponent<UnityEngine.UI.Text>();
            text.text = newPrompt;
        }
    }
}
