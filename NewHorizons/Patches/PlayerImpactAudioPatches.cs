using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches;

[HarmonyPatch]
public static class PlayerImpactAudioPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerImpactAudio), nameof(PlayerImpactAudio.OnImpact))]
    public static bool PlayerImpactAudio_OnImpact()
    {
        // DeathManager and PlayerResources _invincible stops player dying but you still hear the impact sounds which is annoying so we disable them
        return !InvulnerabilityHandler.Invincible;
    }
}
