using HarmonyLib;

namespace NewHorizons.Patches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerBreathingAudio))]
    public static class PlayerBreathingAudioPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerBreathingAudio.OnWakeUp))]
        public static bool PlayerBreathingAudio_OnWakeUp(PlayerBreathingAudio __instance)
        {
            if (Main.Instance.IsWarpingFromShip || Main.Instance.IsWarpingFromVessel || Main.Instance.DidWarpFromShip || Main.Instance.DidWarpFromVessel)
                return false;
            return true;
        }
    }
}
