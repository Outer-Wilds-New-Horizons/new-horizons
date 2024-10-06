using HarmonyLib;
using NewHorizons.Handlers;

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TimeLoop), nameof(TimeLoop.Start))]
        public static void TimeLoop_Start(TimeLoop __instance)
        {
            // If we took the AWC out of the main system make sure to disable time loop
            if (Main.Instance.CurrentStarSystem != "SolarSystem" && HeldItemHandler.WasAWCTakenFromATP)
            {
                TimeLoop.SetTimeLoopEnabled(false);
            }
        }
    }
}
