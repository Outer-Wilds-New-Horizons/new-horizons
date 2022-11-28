using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class TimeLoopPatches
    {
        /// <summary>
        /// Disables starfield updates
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StarfieldController), nameof(StarfieldController.Update))]
        public static bool StarfieldController_Update() => Main.Instance.TimeLoopEnabled;

        /// <summary>
        /// Disables interloper destruction
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(TempCometCollisionFix), nameof(TempCometCollisionFix.Update))]
        public static bool TempCometCollisionFix_Update() => Main.Instance.TimeLoopEnabled;

        /// <summary>
        /// Disables sun logic
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunController), nameof(SunController.Update))]
        public static bool SunController_Update(SunController __instance) => Main.Instance.TimeLoopEnabled && __instance.isActiveAndEnabled;

        /// <summary>
        /// Disables sun expansion
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunController), nameof(SunController.UpdateScale))]
        public static bool SunController_UpdateScale(SunController __instance) => Main.Instance.TimeLoopEnabled && __instance.isActiveAndEnabled;

        /// <summary>
        /// Disables sun collapse SFX
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SunController), nameof(SunController.OnTriggerSupernova))]
        public static bool SunController_OnTriggerSupernova(SunController __instance) => Main.Instance.TimeLoopEnabled && __instance.isActiveAndEnabled;

        /// <summary>
        /// Disables end times music
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GlobalMusicController), nameof(GlobalMusicController.UpdateEndTimesMusic))]
        public static bool GlobalMusicController_UpdateEndTimesMusic() => Main.Instance.TimeLoopEnabled;
    }
}
