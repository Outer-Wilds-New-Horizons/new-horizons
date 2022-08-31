using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public static class RaftingAchievement
    {
        public static readonly string UNIQUE_ID = "NH_RAFTING";

        public static void Init()
        {
            AchievementHandler.Register(UNIQUE_ID, false, Main.Instance);
        }

        public static void Earn()
        {
            AchievementHandler.Earn(UNIQUE_ID);
        }
    }
}
