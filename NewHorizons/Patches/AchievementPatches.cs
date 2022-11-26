using HarmonyLib;
using NewHorizons.OtherMods.AchievementsPlus.NH;
using System.Linq;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class AchievementPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
        public static void DeathManager_KillPlayer(DeathType deathType)
        {
            if (deathType == DeathType.Energy && Locator.GetPlayerDetector().GetComponent<FluidDetector>()._activeVolumes.Any(fluidVolume => fluidVolume is TornadoFluidVolume or TornadoBaseFluidVolume or HurricaneFluidVolume))
            {
                SuckedIntoLavaByTornadoAchievement.Earn();
            }
        }
    }
}
