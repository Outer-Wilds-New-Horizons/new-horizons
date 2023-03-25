using NewHorizons.External.Configs;
using NewHorizons.Utility.OWML;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewHorizons.OtherMods.AchievementsPlus
{
    public static class AchievementHandler
    {
        public static bool Enabled { get; private set; }

        private static IAchievements API;

        private static List<AchievementInfo> _achievements;
       
        public static void Init()
        {
            try
            {
                API = Main.Instance.ModHelper.Interaction.TryGetModApi<IAchievements>("xen.AchievementTracker");

                if (API == null)
                {
                    NHLogger.LogVerbose("Achievements+ isn't installed");
                    Enabled = false;
                    return;
                }

                Enabled = true;

                _achievements = new List<AchievementInfo>();

                // Register base NH achievements
                NH.WarpDriveAchievement.Init();
                NH.VesselWarpAchievement.Init();
                NH.MultipleSystemAchievement.Init();
                NH.EatenOutsideBrambleAchievement.Init();
                NH.NewFrequencyAchievement.Init();
                NH.ProbeLostAchievement.Init();
                NH.RaftingAchievement.Init();
                NH.TalkToFiveCharactersAchievement.Init();
                NH.SuckedIntoLavaByTornadoAchievement.Init();

                API.RegisterTranslationsFromFiles(Main.Instance, "Assets/translations");

                GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
            }
            catch(Exception ex)
            {
                NHLogger.LogError($"Achievements+ handler failed to initialize: {ex}");
            }
        }

        public static void OnDestroy()
        {
            if (!Enabled) return;

            GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
        }

        public static void RegisterAddon(AddonConfig addon, ModBehaviour mod)
        {
            if (addon.achievements == null) return;

            if (!Enabled) return;

            foreach (var achievement in addon.achievements)
            {
                _achievements.Add(achievement);

                API.RegisterAchievement(achievement.ID, achievement.secret, mod);
            }
        }

        public static void RegisterTranslationsFromFiles(ModBehaviour mod, string path) => API.RegisterTranslationsFromFiles(mod, path);

        public static void Earn(string unique_id)
        {
            if (!Enabled) return;

            API.EarnAchievement(unique_id);
        }

        public static void Register(string unique_id, bool secret, ModBehaviour mod)
        {
            if (!Enabled) return;

            API.RegisterAchievement(unique_id, secret, mod);
        }

        public static bool HasAchievement(string unique_id)
        {
            if (!Enabled) return false;

            return API.HasAchievement(unique_id);
        }

        public static void UpdateProgess(string unique_id, int current, int final, bool showPopup)
        {
            if (!Enabled) return;

            API.UpdateProgress(unique_id, current, final, showPopup);
        }

        public static int GetProgress(string unique_id)
        {
            if (!Enabled) return 0;

            return API.GetProgress(unique_id);
        }

        public static void OnLearnSignal()
        {
            if (!Enabled) return;

            foreach (var achievement in _achievements.Where(x => x.signalIDs != null))
            {
                CheckAchievement(achievement);
            }
        }

        public static void OnRevealFact()
        {
            if (!Enabled) return;

            foreach (var achievement in _achievements.Where(x => x.factIDs != null))
            {
                CheckAchievement(achievement);
            }
        }

        public static void OnDialogueConditionChanged(string s, bool b) => OnSetCondition();

        public static void OnSetCondition()
        {
            if (!Enabled) return;

            foreach (var achievement in _achievements.Where(x => x.conditionIDs != null))
            {
                CheckAchievement(achievement);
            }
        }

        private static void CheckAchievement(AchievementInfo achievement)
        {
            if (!Enabled) return;

            if (API.HasAchievement(achievement.ID)) return;

            if (achievement.IsUnlocked())
            {
                Earn(achievement.ID);
            }
        }
    }
}
