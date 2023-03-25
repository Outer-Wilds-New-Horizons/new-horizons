namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public static class NewFrequencyAchievement
    {
        public static readonly string UNIQUE_ID = "NH_NEW_FREQ";

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
