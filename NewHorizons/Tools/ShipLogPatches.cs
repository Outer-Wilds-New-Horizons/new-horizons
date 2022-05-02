using NewHorizons.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Handlers;

namespace NewHorizons.Tools
{
    public static class ShipLogPatches
    {
        public static void Apply()
        {
            var playerDataGetNewlyRevealedFactIDs = typeof(PlayerData).GetMethod("GetNewlyRevealedFactIDs");
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix(playerDataGetNewlyRevealedFactIDs, typeof(ShipLogPatches), nameof(ShipLogPatches.OnPlayerDataGetNewlyRevealedFactIDsComplete));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("Awake", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogManagerAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("Start", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogManagerStart));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("IsFactRevealed", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogManagerIsFactRevealed));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogManager>("CheckForCompletionAchievement", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogManagerCheckForCompletionAchievement));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<UIStyleManager>("GetCuriosityColor", typeof(ShipLogPatches), nameof(ShipLogPatches.OnUIStyleManagerGetCuriosityColor));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogSandFunnel>("Awake", typeof(ShipLogPatches), nameof(ShipLogPatches.DisableShipLogSandFunnel));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogSandFunnel>("UpdateState", typeof(ShipLogPatches), nameof(ShipLogPatches.DisableShipLogSandFunnel));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogAstroObject>("GetName", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogAstroObjectGetName));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogMapMode>("Initialize", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogMapModeInitialize));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogManager>("Awake", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogManagerAwakeComplete));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogAstroObject>("UpdateState", typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogAstroObjectUpdateState));

            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogManager>(nameof(ShipLogManager.RevealFact), typeof(ShipLogPatches), nameof(ShipLogPatches.OnShipLogManagerRevealFact));
        }

        public static void OnShipLogManagerAwake(ShipLogManager __instance)
        {
            RumorModeBuilder.Init();
            ShipLogHandler.Init();
            Logger.Log("Beginning Ship Log Generation For: " + Main.Instance.CurrentStarSystem, Logger.LogType.Log);
            if (Main.Instance.CurrentStarSystem != "SolarSystem")
            {
                __instance._shipLogXmlAssets = new TextAsset[] { };
                foreach (ShipLogEntryLocation logEntryLocation in GameObject.FindObjectsOfType<ShipLogEntryLocation>())
                {
                    logEntryLocation._initialized = true;
                }
            }

            foreach (NewHorizonsBody body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                if (body.Config.ShipLog?.curiosities != null)
                {
                    RumorModeBuilder.AddCuriosityColors(body.Config.ShipLog.curiosities);
                }
            }

            foreach (NewHorizonsBody body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                if (body.Config.ShipLog?.xmlFile != null)
                {
                    RumorModeBuilder.AddBodyToShipLog(__instance, body);
                }
            }
        }

        public static void OnShipLogManagerAwakeComplete(ShipLogManager __instance)
        {
            ShipLogHandler.CheckForModdedFacts(__instance);
            RumorModeBuilder.GenerateEntryData(__instance);
            for (var i = 0; i < __instance._entryList.Count; i++)
            {
                ShipLogEntry logEntry = __instance._entryList[i];
                RumorModeBuilder.UpdateEntryCuriosity(ref logEntry);
            }

            Logger.Log("Ship Log Generation Complete For: " + Main.Instance.CurrentStarSystem, Logger.LogType.Log);
        }

        public static bool OnShipLogManagerIsFactRevealed(ShipLogManager __instance, ref bool __result, string __0)
        {
            if (__instance._factDict != null && __instance._factDict.ContainsKey(__0))
            {
                __result = __instance._factDict[__0].IsRevealed();
            }
            else
            {
                __result = false;
            }

            return false;
        }

        public static bool OnShipLogManagerCheckForCompletionAchievement(ShipLogManager __instance)
        {
            foreach (KeyValuePair<string, ShipLogFact> keyValuePair in __instance._factDict)
            {
                if (ShipLogHandler.IsVanillaAstroID(__instance.GetEntry(keyValuePair.Value.GetEntryID()).GetAstroObjectID()) && !keyValuePair.Value.IsRumor() && !keyValuePair.Value.IsRevealed() && !keyValuePair.Key.Equals("TH_VILLAGE_X3") && !keyValuePair.Key.Equals("GD_GABBRO_ISLAND_X1") && __instance.GetEntry(keyValuePair.Value.GetEntryID()).GetCuriosityName() != CuriosityName.InvisiblePlanet)
                {
                    return false;
                }
            }
            Achievements.Earn(Achievements.Type.STUDIOUS);
            return false;
        }

        public static bool OnShipLogManagerStart(ShipLogManager __instance)
        {
            foreach (NewHorizonsBody body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                foreach (string fact in body.Config.ShipLog?.initialReveal ?? Array.Empty<string>())
                {
                    __instance.RevealFact(fact, false, false);
                }
            }

            if (Main.Instance.CurrentStarSystem == "SolarSystem")
            {
                return true;
            }
            else
            {
                EntryLocationBuilder.InitializeLocations();
                return false;
            }
        }

        public static bool OnUIStyleManagerGetCuriosityColor(UIStyleManager __instance, CuriosityName __0, bool __1, ref Color __result)
        {
            if ((int) __0 < 7)
            {
                return true;
            }
            else
            {
                __result = RumorModeBuilder.GetCuriosityColor(__0, __1, __instance._neutralColor, __instance._neutralHighlight);
                return false;
            }
        }

        public static void OnShipLogMapModeInitialize(ShipLogMapMode __instance)
        {
            GameObject panRoot = GameObject.Find(ShipLogHandler.PAN_ROOT_PATH);
            GameObject sunObject = GameObject.Find(ShipLogHandler.PAN_ROOT_PATH + "/Sun");
            ShipLogAstroObject[][] navMatrix = MapModeBuilder.ConstructMapMode(Main.Instance.CurrentStarSystem, panRoot, __instance._astroObjects, sunObject.layer);
            if (navMatrix == null || navMatrix.Length <= 1)
            {
                Logger.LogWarning("Skipping Map Mode Generation.");
            }
            else
            {
                __instance._astroObjects = navMatrix;
                __instance._startingAstroObjectID = navMatrix[1][0].GetID();
                if (Main.Instance.CurrentStarSystem != "SolarSystem")
                {
                    List<GameObject> delete = SearchUtilities.GetAllChildren(panRoot).Where(g => g.name.Contains("_ShipLog") == false).ToList();
                    foreach (GameObject gameObject in delete)
                    {
                        Object.Destroy(GameObject.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + gameObject.name));
                    }
                    if (GameObject.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + "SandFunnel") == null)
                    {
                        __instance._sandFunnel = __instance.gameObject.AddComponent<ShipLogSandFunnel>();
                    }
                }
            }

            Logger.Log("Map Mode Construction Complete", Logger.LogType.Log);
        }

        public static bool OnShipLogAstroObjectGetName(ShipLogAstroObject __instance, ref string __result)
        {
            if (ShipLogHandler.IsVanillaAstroID(__instance.GetID()))
            {
                return true;
            }
            else
            {
                __result = MapModeBuilder.GetAstroBodyShipLogName(__instance.GetID());
                return false;
            }
        }

        public static void OnShipLogAstroObjectUpdateState(ShipLogAstroObject __instance)
        {
            Transform detailsParent = __instance.transform.Find("Details");
            if (detailsParent != null)
            {
                foreach (GameObject child in SearchUtilities.GetAllChildren(detailsParent.gameObject))
                {
                    Component detail;
                    if (child.TryGetComponent(typeof(ShipLogDetail), out detail))
                    {
                        (detail as ShipLogDetail)?.UpdateState(__instance._state);
                    }
                }
            }

            Transform lineObject = __instance.transform.Find("Line_ShipLog");
            if (lineObject != null)
            {
                ShipLogDetail lineDetail = lineObject.gameObject.GetComponent<ShipLogDetail>();
                lineDetail.UpdateState(__instance._state);
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

        public static void OnShipLogManagerRevealFact(string __0)
        {
            StarChartHandler.OnRevealFact(__0);
        }
    }
}