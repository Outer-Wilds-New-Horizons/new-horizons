using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class RepairVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The name displayed in the UI when the player is repairing this object. If not set, the name of the object will be used.
        /// </summary>
        public string name;

        /// <summary>
        /// How much of the object is initially repaired. 0 = not repaired, 1 = fully repaired.
        /// </summary>
        [DefaultValue(0f)] public float repairFraction = 0f;

        /// <summary>
        /// The time it takes to repair the object. Defaults to 3 seconds.
        /// </summary>
        [DefaultValue(3f)] public float repairTime = 3f;

        /// <summary>
        /// The distance from the object that the player can be to repair it. Defaults to 3 meters.
        /// </summary>
        [DefaultValue(3f)] public float repairDistance = 3f;

        /// <summary>
        /// A dialogue condition that will be set while the object is damaged. It will be unset when the object is repaired.
        /// </summary>
        public string damagedCondition;

        /// <summary>
        /// A dialogue condition that will be set when the object is repaired. It will be unset if the object is damaged again.
        /// </summary>
        public string repairedCondition;

        /// <summary>
        /// A ship log fact that will be revealed when the object is repaired.
        /// </summary>
        public string revealFact;
    }
}
