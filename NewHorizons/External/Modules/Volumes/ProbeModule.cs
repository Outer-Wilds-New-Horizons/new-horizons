using NewHorizons.External.Modules.Volumes.VolumeInfos;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class ProbeModule
    {
        /// <summary>
        /// Add probe destruction volumes to this planet.
        /// These will delete your probe just like the eye of the universe does.
        /// </summary>
        public VolumeInfo[] destructionVolumes;

        /// <summary>
        /// Add probe safety volumes to this planet.
        /// These will stop the probe destruction volumes from working.
        /// </summary>
        public VolumeInfo[] safetyVolumes;
    }
}
