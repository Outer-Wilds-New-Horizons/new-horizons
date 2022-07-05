using HarmonyLib;
using NewHorizons.AchievementsPlus;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Components;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class ShipLogPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        public static void ShipLogManager_Awake_Prefix(ShipLogManager __instance)
        {
            RumorModeBuilder.Init();
            ShipLogHandler.Init();
            Logger.Log($"Beginning Ship Log Generation For: {Main.Instance.CurrentStarSystem}");
            if (Main.Instance.CurrentStarSystem != "SolarSystem")
            {
                __instance._shipLogXmlAssets = new TextAsset[] { };
                foreach (ShipLogEntryLocation logEntryLocation in GameObject.FindObjectsOfType<ShipLogEntryLocation>())
                {
                    logEntryLocation._initialized = true;
                }
            }

            var curiosities = Main.SystemDict[Main.Instance.CurrentStarSystem].Config.curiosities;
            if (curiosities != null)
            {
                RumorModeBuilder.AddCuriosityColors(curiosities);
            }

            foreach (NewHorizonsBody body in Main.BodyDict[Main.Instance.CurrentStarSystem])
            {
                if (body.Config.ShipLog?.xmlFile != null)
                {
                    RumorModeBuilder.AddBodyToShipLog(__instance, body);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        public static void ShipLogManager_Awake_Postfix(ShipLogManager __instance)
        {
            ShipLogHandler.CheckForModdedFacts(__instance);
            RumorModeBuilder.GenerateEntryData(__instance);
            for (var i = 0; i < __instance._entryList.Count; i++)
            {
                ShipLogEntry logEntry = __instance._entryList[i];
                RumorModeBuilder.UpdateEntryCuriosity(ref logEntry);
            }

            Logger.Log($"Ship Log Generation Complete For: {Main.Instance.CurrentStarSystem}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.IsFactRevealed))]
        public static bool ShipLogManager_IsFactRevealed(ShipLogManager __instance, ref bool __result, string __0)
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.CheckForCompletionAchievement))]
        public static bool ShipLogManager_CheckForCompletionAchievement(ShipLogManager __instance)
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Start))]
        public static bool ShipLogManager_Start(ShipLogManager __instance)
        {
            var initialReveal = Main.SystemDict[Main.Instance.CurrentStarSystem].Config.initialReveal ?? Array.Empty<string>();
            foreach (string fact in initialReveal)
            {
                __instance.RevealFact(fact, false, false);
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIStyleManager), nameof(UIStyleManager.GetCuriosityColor))]
        public static bool UIStyleManager_GetCuriosityColor(UIStyleManager __instance, CuriosityName __0, bool __1, ref Color __result)
        {
            if ((int)__0 < 7)
            {
                return true;
            }
            else
            {
                __result = RumorModeBuilder.GetCuriosityColor(__0, __1, __instance._neutralColor, __instance._neutralHighlight);
                return false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogMapMode), nameof(ShipLogMapMode.Initialize))]
        public static void ShipLogMapMode_Initialize(ShipLogMapMode __instance)
        {
            GameObject panRoot = SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH);
            GameObject sunObject = SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/Sun");
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
                    List<GameObject> delete = panRoot.GetAllChildren().Where(g => g.name.Contains("_ShipLog") == false).ToList();
                    foreach (GameObject gameObject in delete)
                    {
                        Object.Destroy(SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + gameObject.name));
                    }
                    if (SearchUtilities.Find(ShipLogHandler.PAN_ROOT_PATH + "/" + "SandFunnel") == null)
                    {
                        __instance._sandFunnel = __instance.gameObject.AddComponent<ShipLogSandFunnel>();
                    }
                }
            }

            Logger.Log("Map Mode Construction Complete");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogAstroObject), nameof(ShipLogAstroObject.GetName))]
        public static bool ShipLogAstroObject_GetName(ShipLogAstroObject __instance, ref string __result)
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogAstroObject), nameof(ShipLogAstroObject.UpdateState))]
        public static void ShipLogAstroObject_UpdateState(ShipLogAstroObject __instance)
        {
            Transform detailsParent = __instance.transform.Find("Details");
            if (detailsParent != null)
            {
                foreach (GameObject child in detailsParent.gameObject.GetAllChildren())
                {
                    if (child.TryGetComponent(typeof(ShipLogDetail), out Component detail))
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogSandFunnel), nameof(ShipLogSandFunnel.UpdateState))]
        public static bool ShipLogSandFunnel_UpdateState()
        {
            return Main.Instance.CurrentStarSystem == "SolarSystem";
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogSandFunnel), nameof(ShipLogSandFunnel.Awake))]
        public static bool ShipLogSandFunnel_Awake()
        {
            return Main.Instance.CurrentStarSystem == "SolarSystem";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.RevealFact))]
        public static void ShipLogManager_RevealFact(string __0)
        {
            StarChartHandler.OnRevealFact(__0);

            AchievementHandler.OnRevealFact();
        }
    }
}