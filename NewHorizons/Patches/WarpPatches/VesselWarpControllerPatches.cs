using HarmonyLib;
using NewHorizons.Components.Ship;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;

namespace NewHorizons.Patches.WarpPatches
{
    [HarmonyPatch(typeof(VesselWarpController))]
    public static class VesselWarpControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VesselWarpController.WarpVessel))]
        public static bool VesselWarpController_WarpVessel(VesselWarpController __instance, bool debugWarp)
        {
            if (!Main.Instance.IsWarpingFromVessel && TimeLoop.GetLoopCount() < 2)
                Achievements.Earn(Achievements.Type.BEGINNERS_LUCK);

            VesselWarpController.s_playerWarpLocation = new RelativeLocationData(Locator.GetPlayerBody(), __instance.transform);
            VesselWarpController.s_relativeLocationSaved = !debugWarp;

            if (!Main.Instance.IsWarpingFromVessel)
                PlayerData.SaveWarpedToTheEye(TimeLoopUtilities.GetVanillaSecondsRemaining());

            // This is totally letting us see the interior of bramble when warping
            Locator.GetPlayerSectorDetector().RemoveFromAllSectors();
            // This is a very jank workaround to stop us seeing all that #957
            Locator.GetPlayerCamera().farClipPlane = 0;
            LoadManager.EnableAsyncLoadTransition();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VesselWarpController.CheckSystemActivation))]
        public static void VesselWarpController_CheckSystemActivation(VesselWarpController __instance)
        {
            // Base method only manages the state of the source warp platform blackhole if the EyeStateManager isn't null
            // However we place the vessel into other systems, so we want to handle the state in those locations as well
            // For some reason the blackhole can also just be null in which case we ignore all this. Happens in Intervention 1.0.3
            if (Locator.GetEyeStateManager() == null && Main.Instance.CurrentStarSystem != "EyeOfTheUniverse" && __instance._sourceWarpPlatform._blackHole != null)
            {
                var isBlackHoleOpen = __instance._sourceWarpPlatform.IsBlackHoleOpen();
                var shouldBlackHoleBeOpen = __instance._hasPower && __instance._warpPlatformPowerSlot.IsActivated() && __instance._targetWarpPlatform != null;

                if (isBlackHoleOpen && !shouldBlackHoleBeOpen)
                {
                    __instance._sourceWarpPlatform.CloseBlackHole();
                }
                else if (!isBlackHoleOpen && shouldBlackHoleBeOpen)
                {
                    __instance._sourceWarpPlatform.OpenBlackHole(__instance._targetWarpPlatform, true);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(VesselWarpController.OnSlotActivated))]
        public static bool VesselWarpController_OnSlotActivated(VesselWarpController __instance, NomaiInterfaceSlot slot)
        {
            bool canWarpToEye = __instance._coordinateInterface.CheckEyeCoordinates();
            bool canWarpToStarSystem = __instance._coordinateInterface.CheckAllCoordinates(out string targetSystem);
            if (slot == __instance._warpVesselSlot && __instance._hasPower && 
                (canWarpToEye && Main.Instance.CurrentStarSystem != "EyeOfTheUniverse" 
                || canWarpToStarSystem && targetSystem != Main.Instance.CurrentStarSystem 
                    && VesselWarpHandler.CanExitViaVessel() && VesselWarpHandler.CanEnterViaVessel(targetSystem)) 
                && __instance._blackHole.GetState() == SingularityController.State.Collapsed)
            {
                __instance._blackHole.Create();
                RumbleManager.StartVesselWarp();
                __instance._openingBlackHole = true;
                __instance.enabled = true;
                Locator.GetPauseCommandListener().AddPauseCommandLock();
                if (canWarpToEye || canWarpToStarSystem && targetSystem == "EyeOfTheUniverse")
                {
                    Main.Instance.CurrentStarSystem = "EyeOfTheUniverse";
                    LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, false, LoadManager.FadeType.ToBlack);// Mobius had the fade set to white. Doesn't look that good because of the loadng screen being black.
                }
                else if (canWarpToStarSystem && targetSystem != "EyeOfTheUniverse")
                    Main.Instance.ChangeCurrentStarSystemVesselAsync(targetSystem);
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
