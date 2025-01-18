using HarmonyLib;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(TravelerEyeController))]
    public static class TravelerEyeControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TravelerEyeController.OnStartCosmicJamSession))]
        public static bool TravelerEyeController_OnStartCosmicJamSession(TravelerEyeController __instance)
        {
            // Not starting the loop audio here; EyeMusicController will handle that
            __instance._signal.GetOWAudioSource().SetLocalVolume(0f);
            return false;
        }
    }
}
