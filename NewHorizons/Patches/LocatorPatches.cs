using HarmonyLib;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class LocatorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.RegisterCloakFieldController))]
        public static bool Locator_RegisterCloakFieldController()
        {
            return Locator._cloakFieldController == null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isPlayerInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isPlayerInsideCloak(CloakFieldController __instance, ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isPlayerInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isProbeInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isProbeInsideCloak(CloakFieldController __instance, ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isProbeInside;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CloakFieldController), nameof(CloakFieldController.isShipInsideCloak), MethodType.Getter)]
        public static void CloakFieldController_isShipInsideCloak(CloakFieldController __instance, ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isShipInside;
        }
    }
}
