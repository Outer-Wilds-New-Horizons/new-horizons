using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public static class EatenOutsideBrambleAchievement
    {
        public static readonly string UNIQUE_ID = "NH_EATEN_OUTSIDE_BRAMBLE";

        public static void Init()
        {
            AchievementHandler.Register(UNIQUE_ID, false, Main.Instance);
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
        }

        public static void OnPlayerDeath(DeathType death)
        {
            if (death == DeathType.Digestion && !PlayerState.InBrambleDimension()) AchievementHandler.Earn(UNIQUE_ID);
        }
    }
}
