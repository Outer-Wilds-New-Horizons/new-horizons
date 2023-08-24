using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.ProxyPatches;

[HarmonyPatch(typeof(ProxyBody))]
public static class ProxyBodyPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ProxyBody.Awake))]
    public static void ProxyBody_Awake(ProxyBody __instance)
    {
        ProxyHandler.RegisterVanillaProxyBody(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ProxyBody.IsObjectInSupernova))]
    public static bool ProxyBody_IsObjectInSupernova(ProxyBody __instance)
    {
        return Locator.GetSunController() != null;
    }
}
