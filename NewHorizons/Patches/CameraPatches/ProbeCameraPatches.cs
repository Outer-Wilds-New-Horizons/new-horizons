using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.CameraPatches
{
    [HarmonyPatch(typeof(ProbeCamera))]
    public static class ProbeCameraPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ProbeCamera.HasInterference))]
        public static void ProbeCamera_HasInterference(ProbeCamera __instance, ref bool __result)
        {
            __result = __result || (__instance._id != ProbeCamera.ID.PreLaunch && (Components.CloakSectorController.isPlayerInside != Components.CloakSectorController.isProbeInside || !InterferenceHandler.IsPlayerSameAsProbe()));
        }
    }
}
