using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public static class SuckedIntoLavaByTornadoAchievement
    {
        public static readonly string UNIQUE_ID = "NH_SUCKED_INTO_LAVA_BY_TORNADO";

        public static void Init()
        {
            AchievementHandler.Register(UNIQUE_ID, false, Main.Instance);
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
        }

        public static void OnPlayerDeath(DeathType deathType)
        {
            if (deathType == DeathType.Lava && Locator.GetPlayerDetector().GetComponent<FluidDetector>()._activeVolumes.Any(fluidVolume => fluidVolume is TornadoFluidVolume or TornadoBaseFluidVolume or HurricaneFluidVolume))
            {
                AchievementHandler.Earn(UNIQUE_ID);
            }
        }
    }
}
