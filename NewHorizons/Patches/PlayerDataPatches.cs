using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.External;
using NewHorizons.Handlers;
using System.Collections.Generic;
using System.Linq;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class PlayerDataPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.KnowsFrequency))]
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.LearnFrequency))]
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.KnowsSignal))]
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.LearnSignal))]
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.KnowsMultipleFrequencies))]
        public static bool OnPlayerDataKnowsMultipleFrequencies(ref bool __result)
        {
            if (NewHorizonsData.KnowsMultipleFrequencies())
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.AddNewlyRevealedFactID))]
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static bool OnPlayerDataGetNewlyRevealedFactIDs(ref List<string> __result)
        {
            __result = PlayerData._currentGameSave.newlyRevealedFactIDs.Concat(NewHorizonsData.GetNewlyRevealedFactIDs()).ToList();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.ClearNewlyRevealedFactIDs))]
        public static bool OnPlayerDataClearNewlyRevealedFactIDs()
        {
            PlayerData._currentGameSave.newlyRevealedFactIDs.Clear();
            NewHorizonsData.ClearNewlyRevealedFactIDs();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.ResetGame))]
        public static void OnPlayerDataResetGame()
        {
            NewHorizonsData.Reset();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static void PlayerData_GetNewlyRevealedFactIDs(ref List<string> __result)
        {
            ShipLogManager manager = Locator.GetShipLogManager();
            __result = __result.Where(e => manager.GetFact(e) != null).ToList();
        }
    }
}
