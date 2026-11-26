using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class ConditionTriggerVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The name of the dialogue condition or persistent condition to set when entering the volume.
        /// </summary>
        public string condition;

        /// <summary>
        /// If true, the condition will persist across all future loops until unset.
        /// </summary>
        public bool persistent;

        /// <summary>
        /// Whether to unset the condition when existing the volume.
        /// </summary>
        public bool reversible;

        /// <summary>
        /// Whether to set the condition when the player enters this volume. Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool player = true;

        /// <summary>
        /// Whether to set the condition when the scout probe enters this volume.
        /// </summary>
        public bool probe;

        /// <summary>
        /// Whether to set the condition when the ship enters this volume.
        /// </summary>
        public bool ship;
    }
}
