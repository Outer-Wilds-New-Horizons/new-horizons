using HarmonyLib;

namespace NewHorizons.Patches.SignalPatches
{
    [HarmonyPatch(typeof(SignalscopeReticleController))]
    public static class SignalScopeReticleControllerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SignalscopeReticleController.UpdateBrackets))]
        public static void SignalscopeReticleController_UpdateBrackets(SignalscopeReticleController __instance)
        {
            var listSignals = Locator.GetAudioSignals();
            for (int i = listSignals.Count; i < __instance._clonedLeftBrackets.Count; i++)
            {
                __instance._clonedLeftBrackets[i].enabled = false;
                __instance._clonedRightBrackets[i].enabled = false;
            }
        }
    }
}
