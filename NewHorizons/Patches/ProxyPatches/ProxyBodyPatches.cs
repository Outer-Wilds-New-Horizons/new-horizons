using HarmonyLib;

namespace NewHorizons.Patches.ProxyPatches
{
    [HarmonyPatch(typeof(ProxyBody))]
    public static class ProxyBodyPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ProxyBody.IsObjectInSupernova))]
        public static bool ProxyBody_IsObjectInSupernova(ProxyBody __instance)
        {
            return Locator.GetSunController() != null;
        }
    }
}
