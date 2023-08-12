using HarmonyLib;

namespace NewHorizons.Patches.ProxyPatches
{
    [HarmonyPatch(typeof(SunProxyEffectController))]
    public static class SunProxyEffectControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SunProxyEffectController.UpdateScales))]
        public static bool SunProxyEffectController_UpdateScales(SunProxyEffectController __instance)
        {
            return __instance != null && __instance._surface != null && __instance._fog != null && __instance._fogMaterial != null && __instance._solarFlareEmitter != null && __instance._atmosphere != null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SunProxyEffectController.UpdateAtmosphereRadii))]
        public static bool SunProxyEffectController_UpdateAtmosphereRadii(SunProxyEffectController __instance)
        {
            return __instance != null && __instance.transform != null && __instance.transform.parent != null && __instance._atmosphereMaterial != null;
        }
    }
}
