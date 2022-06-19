using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NewHorizons.AchievementsPlus;
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
        /// </summary>
        public AchievementInfo[] achievements;

    }
}
