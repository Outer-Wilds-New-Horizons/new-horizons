using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.ProxyPatches;

[HarmonyPatch(typeof(ProxyOrbiter))]
public static class ProxyOrbiterPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ProxyOrbiter.Awake))]
    public static void ProxyOrbiter_Awake(ProxyOrbiter __instance)
    {
        ProxyHandler.RegisterVanillaProxyOrbiter(__instance);
    }
}
