using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class InvincibilityPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
        [HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.ApplyInstantDamage))]
        [HarmonyPatch(typeof(PlayerImpactAudio), nameof(PlayerImpactAudio.OnImpact))]
        public static bool DeathManager_KillPlayer_Prefix()
        {
            // Base game _invincible is still overriden by high speed impacts
            // We also are avoiding playing impact related effects by just skipping these methods
            return !(Locator.GetDeathManager()?._invincible ?? false);
        }
    }
}
