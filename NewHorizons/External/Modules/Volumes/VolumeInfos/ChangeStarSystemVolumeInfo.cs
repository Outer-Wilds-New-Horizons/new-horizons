using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class ChangeStarSystemVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The star system that entering this volume will send you to.
        /// </summary>
        [DefaultValue("SolarSystem")] public string targetStarSystem;

        /// <summary>
        /// ID assigned to a spawn point in the other system that the player will be sent to
        /// Uses the default spawn if not set
        /// </summary>
        public string spawnPointID;
    }
}
