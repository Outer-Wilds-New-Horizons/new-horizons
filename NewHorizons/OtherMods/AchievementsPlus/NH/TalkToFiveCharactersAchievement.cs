using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.OtherMods.AchievementsPlus.NH
{
    public static class TalkToFiveCharactersAchievement
    {
        public static readonly string UNIQUE_ID = "NH_TALK_TO_FIVE_CHARACTERS";

        public static void Init()
        {
            AchievementHandler.Register(UNIQUE_ID, false, Main.Instance);
            UpdateProgress(false);
            if (NewHorizonsData.HasTalkedToFiveCharacters()) Earn();
        }

        public static void OnTalkedToCharacter(string name)
        {
            NewHorizonsData.OnTalkedToCharacter(name);
            if (NewHorizonsData.HasTalkedToFiveCharacters())
            {
                UpdateProgress(false);
                Earn();
            }
            else
                UpdateProgress(true);
        }

        public static void Earn()
        {
            AchievementHandler.Earn(UNIQUE_ID);
        }

        public static void UpdateProgress(bool showPopup)
        {
            AchievementHandler.UpdateProgess(UNIQUE_ID, NewHorizonsData.GetCharactersTalkedTo(), 5, showPopup);
        }
    }
}
