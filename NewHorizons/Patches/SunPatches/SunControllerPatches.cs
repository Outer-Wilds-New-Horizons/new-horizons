using HarmonyLib;

namespace NewHorizons.Patches.SunPatches
{
    [HarmonyPatch(typeof(SunController))]
    public static class SunControllerPatches
    {
        /// <summary>
        /// Disables sun logic if no time loop
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SunController.Update))]
        [HarmonyPatch(nameof(SunController.UpdateScale))]
        [HarmonyPatch(nameof(SunController.OnTriggerSupernova))]
        public static bool SunController_DisableWithoutTimeLoop(SunController __instance) => Main.Instance.TimeLoopEnabled && __instance.isActiveAndEnabled;

    }
}
