using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class CloakPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.FixedUpdate))]
        public static bool CloakFieldController_FixedUpdate(CloakFieldController __instance)
        {
            return __instance != null && __instance._cloakSphereShape != null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isPlayerInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isPlayerInsideCloak(ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isPlayerInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isProbeInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isProbeInsideCloak(ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isProbeInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isShipInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isShipInsideCloak(ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isShipInside;
        }
    }
}
