using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.AchievementsPlus.NH
{
    public static class WarpDriveAchievement
    {
        public static readonly string UNIQUE_ID = "NH_WARP_DRIVE";

        public static void Init()
        {
            AchievementHandler.Register(UNIQUE_ID, false, Main.Instance);
            Main.Instance.OnChangeStarSystem.AddListener(OnChangeStarSystem);
        }

        private static void OnChangeStarSystem(string system)
        {
            if (Main.Instance.IsWarping) AchievementHandler.Earn(UNIQUE_ID);
        }
    }
}
