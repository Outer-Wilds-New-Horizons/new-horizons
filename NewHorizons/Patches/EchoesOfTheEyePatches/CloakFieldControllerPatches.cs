using HarmonyLib;
using NewHorizons.Components.Sectored;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(CloakFieldController))]
    public static class CloakFieldControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CloakFieldController.FixedUpdate))]
        public static bool CloakFieldController_FixedUpdate(CloakFieldController __instance)
        {
            return __instance != null && __instance._cloakSphereShape != null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CloakFieldController.isPlayerInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isPlayerInsideCloak(ref bool __result)
        {
            __result = __result || CloakSectorController.isPlayerInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CloakFieldController.isProbeInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isProbeInsideCloak(ref bool __result)
        {
            __result = __result || CloakSectorController.isProbeInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CloakFieldController.isShipInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isShipInsideCloak(ref bool __result)
        {
            __result = __result || CloakSectorController.isShipInside;
        }
    }
}
