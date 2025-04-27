using NewHorizons.External.Modules.Props;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class ForceVolumeInfo : PriorityVolumeInfo
    {
        /// <summary>
        /// The force applied by this volume. Can be negative to reverse the direction.
        /// </summary>
        public float force;

        /// <summary>
        /// The priority of this force volume for the purposes of alignment.
        /// 
        /// Volumes of higher priority will override volumes of lower priority. Volumes of the same priority will stack like normal.
        /// Ex: A player in a gravity volume with priority 0, and zero-gravity volume with priority 1, will feel zero gravity.
        ///
        /// Default value here is 1 instead of 0 so it automatically overrides planet gravity, which is 0 by default. 
        /// </summary>
        [DefaultValue(1)] public int alignmentPriority = 1;

        /// <summary>
        /// Whether this force volume is inheritable. The most recently activated inheritable force volume will stack with other force volumes even if their priorities differ.
        /// </summary>
        public bool inheritable;
    }
}
