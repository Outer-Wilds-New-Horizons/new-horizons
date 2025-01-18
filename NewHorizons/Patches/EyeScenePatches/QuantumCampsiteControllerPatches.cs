using HarmonyLib;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using System.Linq;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(QuantumCampsiteController))]
    public static class QuantumCampsiteControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(QuantumCampsiteController.Start))]
        public static void QuantumCampsiteController_Start()
        {
            EyeSceneHandler.UpdateTravelerPositions();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(QuantumCampsiteController.ActivateRemainingInstrumentZones))]
        public static void QuantumCampsiteController_ActivateRemainingInstrumentZones(QuantumCampsiteController __instance)
        {
            // We modify this array when registering a custom instrument zone but the vanilla method only activates the first 6
            for (int i = 6; i < __instance._instrumentZones.Length; i++)
            {
                __instance._instrumentZones[i].SetActive(true);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(QuantumCampsiteController.AreAllTravelersGathered))]
        public static bool QuantumCampsiteController_AreAllTravelersGathered(QuantumCampsiteController __instance, ref bool __result)
        {
            bool gatheredAllHearthianTravelers = __instance._travelerControllers.Take(4).All(t => t.gameObject.activeInHierarchy);
            if (!gatheredAllHearthianTravelers)
            {
                NHLogger.LogVerbose("");
                __result = false;
                return false;
            }
            bool needsSolanum = __instance._hasMetSolanum;
            bool gatheredSolanum = __instance._travelerControllers[QuantumCampsiteController.SOLANUM_INDEX].gameObject.activeInHierarchy;
            if (needsSolanum && !gatheredSolanum)
            {
                __result = false;
                return false;
            }
            bool needsPrisoner = __instance._hasMetPrisoner && !__instance._hasErasedPrisoner;
            bool gatheredPrisoner = __instance._travelerControllers[QuantumCampsiteController.PRISONER_INDEX].gameObject.activeInHierarchy;
            if (needsPrisoner && !gatheredPrisoner)
            {
                __result = false;
                return false;
            }
            foreach (var traveler in EyeSceneHandler.GetCustomEyeTravelers())
            {
                bool needsTraveler = true;
                if (!string.IsNullOrEmpty(traveler.info.participatingCondition))
                {
                    needsTraveler = DialogueConditionManager.SharedInstance.GetConditionState(traveler.info.participatingCondition);
                }
                bool gatheredTraveler = traveler.controller.gameObject.activeInHierarchy;
                if (needsTraveler && !gatheredTraveler)
                {
                    __result = false;
                    return false;
                }
            }
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(QuantumCampsiteController.OnTravelerStartPlaying))]
        public static void OnTravelerStartPlaying(QuantumCampsiteController __instance)
        {
            if (!__instance._hasJamSessionStarted)
            {
                NHLogger.Log($"NH is handling Eye sequence music");
                // Jam session is starting, start our custom music handler
                EyeSceneHandler.GetMusicController().StartPlaying();
            }
            // Letting the original method run in case mods have patched TravelerEyeController.OnStartCosmicJamSession()
        }
    }
}
