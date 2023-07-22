using NewHorizons.Components.Achievement;

namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public class RaftingAchievement : AchievementVolume
    {
        public static readonly string UNIQUE_ID = "NH_RAFTING";

        private void Awake()
        {
            achievementID = UNIQUE_ID;
        }

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
