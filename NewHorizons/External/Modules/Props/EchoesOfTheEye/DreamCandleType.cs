using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DreamCandleType
    {
        [EnumMember(Value = @"ground")] Ground,

        [EnumMember(Value = @"groundSmall")] GroundSmall,

        [EnumMember(Value = @"groundLarge")] GroundLarge,

        [EnumMember(Value = @"groundSingle")] GroundSingle,

        [EnumMember(Value = @"wall")] Wall,

        [EnumMember(Value = @"wallLargeFlame")] WallLargeFlame,

        [EnumMember(Value = @"wallBigWick")] WallBigWick,

        [EnumMember(Value = @"standing")] Standing,

        [EnumMember(Value = @"pile")] Pile,
    }
}
