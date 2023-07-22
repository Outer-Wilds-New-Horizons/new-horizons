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
        /// The exception is layer 0. A higher-priority volume in layer 0 will override lower-priority volumes in ALL other layers. A lower-priority volume in layer 0 will stack with other layers like normal.
        ///
        /// Ex: A player could be affected by the sun on layer 9 priority 0 and planet gravity on layer 3 priority 2. They would experience the gravity of both volumes since they are on different layers.
        /// If there was a zero-g volume on layer 0 priority 1, since it is on layer 0 it will override the gravity from the sun (priority 0 which is less than 1) but they will still feel the 
        /// gravity of the planet (priority 2 is greater than 1). The zero-g volume will also still be applied because it is on a different layer.
        ///
        /// Default value here is 0 which means this volume's priority will be evaluated against all other priority volumes regardless of their layer.
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
