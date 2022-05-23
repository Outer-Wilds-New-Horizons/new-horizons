using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class LocatorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Locator), nameof(Locator.RegisterCloakFieldController))]
        public static bool Locator_RegisterCloakFieldController() => Locator._cloakFieldController == null;
    }
}