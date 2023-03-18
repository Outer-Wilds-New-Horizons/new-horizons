using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Volumes
{
    [JsonObject]
    public class ChangeStarSystemVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The star system that entering this volume will send you to.
        /// </summary>
        [DefaultValue("SolarSystem")]
        public string targetStarSystem;
    }
}

