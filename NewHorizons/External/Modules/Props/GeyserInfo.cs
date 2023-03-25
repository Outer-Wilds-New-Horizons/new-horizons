using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class GeyserInfo : GeneralPropInfo
    {
        /// <summary>
        /// Vertical offset of the geyser. From 0, the bubbles start at a height of 10, the shaft at 67, and the spout at 97.5.
        /// </summary>
        [DefaultValue(-97.5f)] public float offset = -97.5f;

        /// <summary>
        /// Force of the geyser on objects
        /// </summary>
        [DefaultValue(55f)] public float force = 55f;

        /// <summary>
        /// Time in seconds eruptions last for
        /// </summary>
        [DefaultValue(10f)] public float activeDuration = 10f;

        /// <summary>
        /// Time in seconds between eruptions
        /// </summary>
        [DefaultValue(19f)] public float inactiveDuration = 19f;

        /// <summary>
        /// Color of the geyser. Alpha sets the particle density.
        /// </summary>
        public MColor tint;

        /// <summary>
        /// Disable the individual particle systems of the geyser
        /// </summary>
        public bool disableBubbles, disableShaft, disableSpout;

        /// <summary>
        /// Loudness of the geyser
        /// </summary>
        [DefaultValue(0.7f)] public float volume = 0.7f;
    }
}
