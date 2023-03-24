using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class TimeLoopPatches
    {
        /// <summary>
        /// Disable certain behaviours without timeloop
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StarfieldController), nameof(StarfieldController.Update))]
        [HarmonyPatch(typeof(TempCometCollisionFix), nameof(TempCometCollisionFix.Update))]
        [HarmonyPatch(typeof(GlobalMusicController), nameof(GlobalMusicController.UpdateEndTimesMusic))]
        [HarmonyPatch(typeof(TimeLoop), nameof(TimeLoop.Update))]
        public static bool DisableWithoutTimeLoop() => Main.Instance.TimeLoopEnabled;
    }
}
