using HarmonyLib;
using NewHorizons.Builder.ShipLog;
using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(ShipLogManager))]
    public static class ShipLogManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogManager.Awake))]
        public static void ShipLogManager_Awake_Prefix(ShipLogManager __instance)
        {
            if (Main.Instance.IsWarpingBackToEye) return;

            RumorModeBuilder.Init();
            ShipLogHandler.Init();

            var currentStarSystem = Main.Instance.CurrentStarSystem;

            if (!Main.SystemDict.ContainsKey(currentStarSystem) || !Main.BodyDict.ContainsKey(currentStarSystem))
            {
                currentStarSystem = Main.Instance.DefaultStarSystem;
            }

            NHLogger.Log($"Beginning Ship Log Generation For: {currentStarSystem}");

            if (currentStarSystem != "SolarSystem" && currentStarSystem != "EyeOfTheUniverse")
            {
                __instance._shipLogXmlAssets = new TextAsset[] { };
                foreach (ShipLogEntryLocation logEntryLocation in Object.FindObjectsOfType<ShipLogEntryLocation>())
                {
                    logEntryLocation._initialized = true;
                }
            }

            var curiosities = Main.SystemDict[currentStarSystem].Config.curiosities;
            if (curiosities != null)
            {
                RumorModeBuilder.AddCuriosityColors(curiosities);
            }

            foreach (NewHorizonsBody body in Main.BodyDict[currentStarSystem])
            {
                if (body.Config.ShipLog?.xmlFile != null)
                {
                    RumorModeBuilder.AddBodyToShipLog(__instance, body);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ShipLogManager.Awake))]
        public static void ShipLogManager_Awake_Postfix(ShipLogManager __instance)
        {
            if (Main.Instance.IsWarpingBackToEye) return;

            ShipLogHandler.CheckForModdedFacts(__instance);
            RumorModeBuilder.GenerateEntryData(__instance);
            for (var i = 0; i < __instance._entryList.Count; i++)
            {
                ShipLogEntry logEntry = __instance._entryList[i];
                RumorModeBuilder.UpdateEntryCuriosity(ref logEntry);
            }

            NHLogger.Log($"Ship Log Generation Complete For: {Main.Instance.CurrentStarSystem}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogManager.IsFactRevealed))]
        public static bool ShipLogManager_IsFactRevealed(ShipLogManager __instance, ref bool __result, string id)
        {
            // normally throws an error on not found
            if (__instance._factDict != null && __instance._factDict.ContainsKey(id))
            {
                __result = __instance._factDict[id].IsRevealed();
            }
            else
            {
                __result = false;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogManager.CheckForCompletionAchievement))]
        public static bool ShipLogManager_CheckForCompletionAchievement(ShipLogManager __instance)
        {
            // In other star systems they won't have the base game ship log facts, so they'll instantly get the achievement.
            if (Main.Instance.CurrentStarSystem != "SolarSystem") return false;

            foreach (KeyValuePair<string, ShipLogFact> keyValuePair in __instance._factDict)
            {
                // Don't want to include modded facts, otherwise this is the same as the vanilla method
                if (!ShipLogHandler.IsModdedFact(keyValuePair.Key) && !keyValuePair.Value.IsRumor() && !keyValuePair.Value.IsRevealed() && !keyValuePair.Key.Equals("TH_VILLAGE_X3") && !keyValuePair.Key.Equals("GD_GABBRO_ISLAND_X1") && __instance.GetEntry(keyValuePair.Value.GetEntryID()).GetCuriosityName() != CuriosityName.InvisiblePlanet)
                {
                    return false;
                }
            }
            Achievements.Earn(Achievements.Type.STUDIOUS);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogManager.Start))]
        public static bool ShipLogManager_Start(ShipLogManager __instance)
        {
            var initialReveal = Main.SystemDict[Main.Instance.CurrentStarSystem].Config.initialReveal ?? System.Array.Empty<string>();
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
                // Start method disables the ShipLogManager
                // Else it thinks its meant to be waiting to post a SHIP LOG UPDATED notif (and then does so) #779
                __instance.enabled = false;
                return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogManager.AddEntry))]
        public static bool ShipLogManager_AddEntry(ShipLogManager __instance, ShipLogEntry entry)
        {
            if (__instance._entryDict.TryGetValue(entry.GetID(), out var existing))
            {
                NHLogger.LogVerbose($"Merging duplicate shiplog entry: {entry.GetID()}");
                RumorModeBuilder.MergeEntries(__instance, entry, existing);
                return false;
            }
            return true;
        }
    }
}
