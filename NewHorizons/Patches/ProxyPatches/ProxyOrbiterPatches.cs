using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.ProxyPatches;

[HarmonyPatch(typeof(ProxyOrbiter))]
public static class ProxyOrbiterPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ProxyOrbiter.SetOriginalBodies))]
    public static void ProxyOrbiter_SetOriginalBodies(ProxyOrbiter __instance)
    {
        ProxyHandler.RegisterVanillaProxyOrbiter(__instance);
    }
}
