using HarmonyLib;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class NomaiCoordinatePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiCoordinateInterface), nameof(NomaiCoordinateInterface.SetPillarRaised), new System.Type[] { typeof(bool) })]
        public static bool NomaiCoordinateInterface_SetPillarRaised(NomaiCoordinateInterface __instance, bool raised)
        {
            if (raised)
                return !(!__instance._powered || (__instance.CheckEyeCoordinates() && Main.Instance.CurrentStarSystem != "EyeOfTheUniverse") || (__instance.CheckAllCoordinates(out string targetSystem) && Main.Instance.CurrentStarSystem != targetSystem));
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VesselWarpController), nameof(VesselWarpController.WarpVessel))]
        public static bool VesselWarpController_WarpVessel(VesselWarpController __instance, bool debugWarp)
        {
            if (!Main.Instance.IsWarpingFromVessel && TimeLoop.GetLoopCount() < 2)
                Achievements.Earn(Achievements.Type.BEGINNERS_LUCK);
            VesselWarpController.s_playerWarpLocation = new RelativeLocationData(Locator.GetPlayerBody(), __instance.transform);
            VesselWarpController.s_relativeLocationSaved = !debugWarp;
            if (!Main.Instance.IsWarpingFromVessel)
                PlayerData.SaveWarpedToTheEye(TimeLoopUtilities.GetVanillaSecondsRemaining());
            LoadManager.EnableAsyncLoadTransition();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VesselWarpController), nameof(VesselWarpController.CheckSystemActivation))]
        public static void VesselWarpController_CheckSystemActivation(VesselWarpController __instance)
        {
            if (Locator.GetEyeStateManager() == null && Main.Instance.CurrentStarSystem != "EyeOfTheUniverse")
            {
                if (!__instance._sourceWarpPlatform.IsBlackHoleOpen() && __instance._hasPower && __instance._warpPlatformPowerSlot.IsActivated() && __instance._targetWarpPlatform != null)
                    __instance._sourceWarpPlatform.OpenBlackHole(__instance._targetWarpPlatform, true);
                else if (__instance._sourceWarpPlatform.IsBlackHoleOpen() && (!__instance._hasPower || !__instance._warpPlatformPowerSlot.IsActivated()))
                    __instance._sourceWarpPlatform.CloseBlackHole();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(VesselWarpController), nameof(VesselWarpController.OnSlotActivated))]
        public static bool VesselWarpController_OnSlotActivated(VesselWarpController __instance, NomaiInterfaceSlot slot)
        {
            bool canWarpToEye = __instance._coordinateInterface.CheckEyeCoordinates();
            bool canWarpToStarSystem = __instance._coordinateInterface.CheckAllCoordinates(out string targetSystem);
            if (slot == __instance._warpVesselSlot && __instance._hasPower && ((canWarpToEye && Main.Instance.CurrentStarSystem != "EyeOfTheUniverse") || (canWarpToStarSystem && targetSystem != Main.Instance.CurrentStarSystem)) && __instance._blackHole.GetState() == SingularityController.State.Collapsed)
            {
                __instance._blackHole.Create();
                RumbleManager.StartVesselWarp();
                __instance._openingBlackHole = true;
                __instance.enabled = true;
                Locator.GetPauseCommandListener().AddPauseCommandLock();
                if (canWarpToEye || (canWarpToStarSystem && targetSystem == "EyeOfTheUniverse"))
                {
                    Main.Instance._currentStarSystem = "EyeOfTheUniverse";
                    LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, false, LoadManager.FadeType.ToWhite);
                }
                else if (canWarpToStarSystem)
                    Main.Instance.ChangeCurrentStarSystem(targetSystem, false, true);
                __instance._blackHoleOneShot.PlayOneShot(AudioType.VesselSingularityCreate);
                GlobalMessenger.FireEvent("StartVesselWarp");
            }
            else
            {
                if (slot == __instance._coordinatePowerSlot)
                    __instance._coordinateInterface.SetPillarRaised(true, true);
                __instance.CheckSystemActivation();
            }
            return false;
        }
    }
}
