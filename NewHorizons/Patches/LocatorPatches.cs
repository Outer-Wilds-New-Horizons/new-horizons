#region

using HarmonyLib;

#endregion

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
    }
}