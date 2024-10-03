using HarmonyLib;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.OtherMods.AchievementsPlus;
using NewHorizons.OtherMods.AchievementsPlus.NH;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Linq;

namespace NewHorizons.Patches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerData))]
    public static class PlayerDataPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.KnowsFrequency))]
        public static bool PlayerData_KnowsFrequency(SignalFrequency frequency, ref bool __result)
        {
            var freqString = SignalBuilder.GetCustomFrequencyName(frequency);
            if (!string.IsNullOrEmpty(freqString))
            {
                __result = NewHorizonsData.KnowsFrequency(freqString);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.LearnFrequency))]
        public static bool PlayerData_LearnFrequency(SignalFrequency frequency)
        {
            var freqString = SignalBuilder.GetCustomFrequencyName(frequency);
            if (!string.IsNullOrEmpty(freqString))
            {
                NewHorizonsData.LearnFrequency(freqString);
                NewFrequencyAchievement.Earn();
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.ForgetFrequency))]
        public static bool PlayerData_ForgetFrequency(SignalFrequency frequency)
        {
            var freqString = SignalBuilder.GetCustomFrequencyName(frequency);
            if (!string.IsNullOrEmpty(freqString))
            {
                NewHorizonsData.ForgetFrequency(freqString);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.KnowsSignal))]
        public static bool PlayerData_KnowsSignal(SignalName signalName, ref bool __result)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(signalName);
            if (!string.IsNullOrEmpty(customSignalName))
            {
                __result = NewHorizonsData.KnowsSignal(customSignalName);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.LearnSignal))]
        public static bool PlayerData_LearnSignal(SignalName signalName)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(signalName);
            if (!string.IsNullOrEmpty(customSignalName))
            {
                NewHorizonsData.LearnSignal(customSignalName);
                AchievementHandler.OnLearnSignal();

                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.KnowsMultipleFrequencies))]
        public static bool PlayerData_KnowsMultipleFrequencies(ref bool __result)
        {
            __result = NewHorizonsData.KnowsMultipleFrequencies();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.AddNewlyRevealedFactID))]
        public static bool PlayerData_AddNewlyRevealedFactID(string id)
        {
            if (ShipLogHandler.IsModdedFact(id))
            {
                NewHorizonsData.AddNewlyRevealedFactID(id);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static bool PlayerData_GetNewlyRevealedFactIDs_Prefix(ref List<string> __result)
        {
            var newHorizonsNewlyRevealedFactIDs = NewHorizonsData.GetNewlyRevealedFactIDs();
            if (newHorizonsNewlyRevealedFactIDs != null)
            {
                __result = PlayerData._currentGameSave.newlyRevealedFactIDs.Concat(newHorizonsNewlyRevealedFactIDs).ToList();
                return false;
            }
            NHLogger.LogError("Newly Revealed Fact IDs is null!");
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static void PlayerData_GetNewlyRevealedFactIDs_Postfix(ref List<string> __result)
        {
            var manager = Locator.GetShipLogManager();
            __result = __result.Where(id => manager.GetFact(id) != null).ToList();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerData.ClearNewlyRevealedFactIDs))]
        public static void PlayerData_ClearNewlyRevealedFactIDs()
        {
            NewHorizonsData.ClearNewlyRevealedFactIDs();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerData.ResetGame))]
        public static void PlayerData_ResetGame()
        {
            NewHorizonsData.Reset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerData.SaveCurrentGame))]
        public static void PlayerData_SaveCurrentGame()
        {
            NewHorizonsData.Save();
        }
    }
}
