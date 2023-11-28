using HarmonyLib;

namespace NewHorizons.Patches.BrittleHollowPatches;

[HarmonyPatch(typeof(TimedFragmentIntegrity))]
internal class TimedFragmentIntegrityPatches
{
    // For some inexplicable reason Mobius decided to implement some weird custom loop using delayed method invoking
    // This starts on Awake and isn't stopped by the object being set inactive
    // Mobius why
    // We just need to have this method not run if the object is inactive, to prevent the fragment being detached and activating its own proxy body
    // While the fragment itself never appears, the proxy does
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimedFragmentIntegrity.OnLatestTimeReached))]
    public static bool TimedFragmentIntegrity_OnLatestTimeReached(TimedFragmentIntegrity __instance)
    {
        return __instance.gameObject.activeInHierarchy;
    }
}
