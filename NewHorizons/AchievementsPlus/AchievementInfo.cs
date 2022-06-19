using NewHorizons.Builder.Props;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.AchievementsPlus
{
    /// <summary>
    /// Info for an achievement to be used with the Achievements+ mod.
    /// </summary>
    [JsonObject]
    public class AchievementInfo
    {
        /// <summary>
        /// The unique ID of the achievement. This must be globally unique, meaning all achivements for
        /// you mod should start with something to identify the mod they are from. For example, Real Solar System
        /// uses "RSS_" and Signals+ would use "SIGNALS_PLUS_".
        /// </summary>
        public string ID;

        /// <summary>
        /// Should the name and description of the achievement be hidden until it is unlocked. Good for hiding spoilers!
        /// </summary>
        public bool secret;

        /// <summary>
        /// A list of facts that must be discovered before this achievement is unlocked. You can also set the achievement
        /// to be unlocked by a reveal trigger in Props -> Reveals. Optional.
        /// </summary>
        public string[] factIDs;

        /// <summary>
        /// A list of signals that must be discovered before this achievement is unlocked. Optional.
        /// </summary>
        public string[] signalIDs;

        /// <summary>
        /// A list of conditions that must be true before this achievement is unlocked. Conditions can be set via dialogue. Optional.
        /// </summary>
        public string[] conditionIDs;

        // Cache signal ids to the enum
        private SignalName[] _signalIDs;

        public bool IsUnlocked()
        {
            if (signalIDs != null)
            {
                if (_signalIDs == null)
                {
                    _signalIDs = signalIDs.Select(x => SignalBuilder.StringToSignalName(x)).ToArray();
                }

                foreach(var signal in _signalIDs)
                {
                    if (!PlayerData.KnowsSignal(signal)) return false;
                }
            }

            if (factIDs != null)
            {
                foreach (var fact in factIDs)
                {
                    if (!Locator.GetShipLogManager().IsFactRevealed(fact)) return false;
                }
            }

            if (conditionIDs != null)
            {
                foreach (var condition in conditionIDs)
                {
                    if (!DialogueConditionManager.SharedInstance.GetConditionState(condition)) return false;
                }
            }

            return true;
        }
    }
}
