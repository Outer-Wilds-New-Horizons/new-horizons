using HarmonyLib;
using NewHorizons.Utility;

namespace NewHorizons.Patches.ProxyPatches
{
    [HarmonyPatch(typeof(SunProxy))]
    public static class SunProxyPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SunProxy.Start))]
        public static void SunProxy_Start(SunProxy __instance)
        {
            // We mess with the Locator.GetSunTransform() value to switch it to other relevant stars since it's used for some other effects
            // However if it's set to a different star when the SunProxy starts it breaks, so we double check it here
            __instance._sunTransform = SearchUtilities.Find("Sun_Body").transform;
            __instance._realSunController = __instance._sunTransform.GetComponent<SunController>();
        }
    }
}
