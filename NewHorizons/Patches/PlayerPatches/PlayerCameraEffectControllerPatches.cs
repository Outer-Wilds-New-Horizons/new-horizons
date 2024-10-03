using HarmonyLib;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Patches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerCameraEffectController))]
    public static class PlayerCameraEffectControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerCameraEffectController.OnStartOfTimeLoop))]
        public static bool PlayerCameraEffectController_OnStartOfTimeLoop(PlayerCameraEffectController __instance, int loopCount)
        {
            if (__instance.gameObject.CompareTag("MainCamera") && (Main.Instance.IsWarpingFromShip || Main.Instance.IsWarpingFromVessel || Main.Instance.DidWarpFromShip || Main.Instance.DidWarpFromVessel))
            {
                __instance.CloseEyesImmediate();
                GlobalMessenger.FireEvent("WakeUp");
                return false;
            }
            return true;
        }
    }
}
