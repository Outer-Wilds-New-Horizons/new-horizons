using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class OxygenVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// Does this volume contain trees? This will change the notification from "Oxygen tank refilled" to "Trees detected, oxygen tank refilled".
        /// </summary>
        public bool treeVolume;

        /// <summary>
        /// Whether to play the oxygen tank refill sound or just fill quietly.
        /// </summary>
        [DefaultValue(true)] public bool playRefillAudio = true;
    }

}
