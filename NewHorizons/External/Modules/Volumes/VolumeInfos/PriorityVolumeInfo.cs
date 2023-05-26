using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class PriorityVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The layer of this volume.
        /// 
        /// Layers separate the priority system. The priority of volumes in one layer will not affect or override volumes in another. The highest priority volume in each layer will stack like normal.
        /// The exception is layer 0. Layer 0 effectively bypasses the layer system. The priority of volumes in layer 0 will override volumes in all other layers, as if they were all in the same layer.
        ///
        /// Default value here is 0 because in most cases people don't want to think about layers and only care about priority.
        /// </summary>
        [DefaultValue(0)] public int layer = 0;

        /// <summary>
        /// The priority of this volume.
        /// 
        /// Volumes of higher priority will override volumes of lower priority. Volumes of the same priority will stack like normal.
        /// Ex: A player in a gravity volume with priority 0, and zero-gravity volume with priority 1, will feel zero gravity.
        ///
        /// Default value here is 1 instead of 0 so it automatically overrides planet gravity, which is 0 by default. 
        /// </summary>
        [DefaultValue(1)] public int priority = 1;
    }
}
