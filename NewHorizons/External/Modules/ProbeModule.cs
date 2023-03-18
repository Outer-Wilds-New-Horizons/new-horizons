using NewHorizons.External.Volumes;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
        public class ProbeModule
        {
            /// <summary>
            /// Add probe destruction volumes to this planet. These will delete your probe.
            /// </summary>
            public VolumeInfo[] destructionVolumes;

            /// <summary>
            /// Add probe safety volumes to this planet. These will stop the probe destruction volumes from working.
            /// </summary>
            public VolumeInfo[] safetyVolumes;
        }
    }

