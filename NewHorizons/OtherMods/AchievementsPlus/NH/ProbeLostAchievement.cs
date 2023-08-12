namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public static class ProbeLostAchievement
    {
        public static readonly string UNIQUE_ID = "NH_PROBE_LOST";

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
