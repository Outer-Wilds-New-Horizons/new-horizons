using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class SpeedTrapVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The speed the volume will slow you down to when you enter it.
        /// </summary>
        [DefaultValue(10f)]
        public float speedLimit = 10f;

        /// <summary>
        /// How fast it will slow down the player to the speed limit.
        /// </summary>
        [DefaultValue(3f)]
        public float acceleration = 3f;
    }
}
