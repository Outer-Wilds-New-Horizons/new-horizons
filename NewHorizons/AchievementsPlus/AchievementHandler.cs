using NewHorizons.Utility;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewHorizons.AchievementsPlus
{
    public static class AchievementHandler
    {
        public static bool Enabled { get => _enabled; }

        private static bool _enabled;
        private static IAchievements API;

        private static List<AchievementInfo> _achievements;
       
        public static void Init()
        {
            API = Main.Instance.ModHelper.Interaction.TryGetModApi<IAchievements>("xen.AchievementTracker");

            if (API == null)
            {
                Logger.Log("Achievements+ isn't installed");
                _enabled = false;
                return;
            }

            _enabled = true;

            // Register base NH achievements
            NH.WarpDriveAchievement.Init();
            NH.MultipleSystemAchievement.Init();
            NH.EatenOutsideBrambleAchievement.Init();
            NH.NewFrequencyAchievement.Init();
            NH.ProbeLostAchievement.Init();

            API.RegisterTranslationsFromFiles(Main.Instance, "Assets/translations");

            GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
        }

        public static void OnDestroy()
        {
            if (!_enabled) return;

            GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
        }

        public static void RegisterTranslationsFromFiles(ModBehaviour mod, string path) => API.RegisterTranslationsFromFiles(mod, path);

        public static void Earn(string unique_id)
        {
            if (!_enabled) return;

            API.EarnAchievement(unique_id);
        }

        public static void Register(string unique_id, bool secret, ModBehaviour mod)
        {
            if (!_enabled) return;

            API.RegisterAchievement(unique_id, secret, mod);
        }

        public static void OnLearnSignal()
        {
            if (!_enabled) return;

            foreach (var achievement in _achievements.Where(x => x.signalIDs != null))
            {
                CheckAchievement(achievement);
            }
        }

        public static void OnRevealFact()
        {
            if (!_enabled) return;

            foreach (var achievement in _achievements.Where(x => x.factIDs != null))
            {
                CheckAchievement(achievement);
            }
        }

        public static void OnDialogueConditionChanged(string s, bool b) => OnSetCondition();

        public static void OnSetCondition()
        {
            if (!_enabled) return;

            foreach (var achievement in _achievements.Where(x => x.conditionIDs != null))
            {
                CheckAchievement(achievement);
            }
        }

        private static void CheckAchievement(AchievementInfo achievement)
        {
            if (!_enabled) return;

            if (API.HasAchievement(achievement.ID)) return;

            if (achievement.IsUnlocked())
            {
                Earn(achievement.ID);
            }
        }
    }
}
