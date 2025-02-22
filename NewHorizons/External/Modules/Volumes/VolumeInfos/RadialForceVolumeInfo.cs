using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class RadialForceVolumeInfo : ForceVolumeInfo
    {
        /// <summary>
        /// How the force falls off with distance. Defaults to linear.
        /// </summary>
        [DefaultValue("linear")] public FallOff fallOff = FallOff.Linear;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum FallOff
        {
            [EnumMember(Value = @"constant")] Constant = 0,

            [EnumMember(Value = @"linear")] Linear = 1,

            [EnumMember(Value = @"inverseSquared")]
            InverseSquared = 2
        }
    }
}
