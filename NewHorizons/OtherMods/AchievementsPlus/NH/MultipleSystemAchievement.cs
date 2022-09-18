using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public static class MultipleSystemAchievement
    {
        public static readonly string UNIQUE_ID = "NH_MULTIPLE_SYSTEM";

        private static List<string> _uniqueSystems = new List<string>();

        public static void Init()
        {
            AchievementHandler.Register(UNIQUE_ID, false, Main.Instance);
            Main.Instance.OnChangeStarSystem.AddListener(OnChangeStarSystem);
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
        }

        public static void OnPlayerDeath(DeathType _)
        {
            if (Main.Instance.IsChangingStarSystem) return;

            _uniqueSystems.Clear();
        }

        public static void OnChangeStarSystem(string system)
        {
            if (_uniqueSystems.Contains(system)) return;
            _uniqueSystems.Add(system);
            if(_uniqueSystems.Count > 5)
            {
                AchievementHandler.Earn(UNIQUE_ID);
                _uniqueSystems.Clear();
            }
        }
    }
}
