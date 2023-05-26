using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class PriorityVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The layer of this volume.
        /// </summary>
        [DefaultValue(0)] public int layer = 0;

        /// <summary>
        /// The priority for this volume's effects to be applied. 
        /// Ex, a player in a gravity volume with priority 0, and zero-gravity volume with priority 1, will feel zero gravity.
        ///
        /// Default value here is 1 instead of 0 so it automatically overrides planet gravity, which is 0 by default. 
        /// </summary>
        [DefaultValue(1)] public int priority = 1;
    }
}
