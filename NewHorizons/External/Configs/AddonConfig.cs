using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NewHorizons.OtherMods.AchievementsPlus;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.VariableSize;
using Newtonsoft.Json;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// Describes the New Horizons addon itself
    /// </summary>
    [JsonObject]
    public class AddonConfig
    {
        /// <summary>
        /// Achievements for this mod if the user is playing with Achievements+
        /// Achievement icons must be put in a folder named "icons"
        /// The icon for the mod must match the name of the mod (e.g., New Horizons.png)
        /// The icons for achievements must match their unique IDs (e.g., NH_WARP_DRIVE.png)
        /// </summary>
        public AchievementInfo[] achievements;

        /// <summary>
        /// Credits info for this mod. A list of contributors and their roles separated by #. For example: xen#New Horizons dev.
        /// </summary>
        public string[] credits;

        /// <summary>
        /// A pop-up message for the first time a user runs the add-on
        /// </summary>
        public string popupMessage;
    }
}
