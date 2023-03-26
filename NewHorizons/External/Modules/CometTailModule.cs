using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class CometTailModule
    {
        /// <summary>
        /// Manually sets the local rotation
        /// </summary>
        public MVector3 rotationOverride;

        /// <summary>
        /// Inner radius of the comet tail, defaults to match surfaceSize
        /// </summary>
        public float? innerRadius;

        /// <summary>
        /// The body that the comet tail should always point away from
        /// </summary>
        public string primaryBody;
    }
}
