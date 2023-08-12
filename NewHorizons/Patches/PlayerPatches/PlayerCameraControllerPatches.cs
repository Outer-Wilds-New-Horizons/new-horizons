using HarmonyLib;

namespace NewHorizons.Patches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerCameraController))]
    public static class PlayerCameraControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerCameraController.SnapToDegreesOverSeconds))]
        public static bool PlayerCameraController_SnapToDegreesOverSeconds(PlayerCameraController __instance, float targetX, float targetY, float duration, bool smoothStep)
        {
            // AlignPlayerWithForce.OnSuitUp snaps the camera to center, but it never unsnaps because duration == 0f
            if (duration <= 0f)
            {
                __instance._degreesX = targetX;
                __instance._degreesY = targetY;
                return false;
            }
            return true;
        }
    }
}
