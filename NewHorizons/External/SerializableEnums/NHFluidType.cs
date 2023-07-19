using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NHFluidType
    {
        [EnumMember(Value = @"none")] NONE,
        [EnumMember(Value = @"air")] AIR,
        [EnumMember(Value = @"water")] WATER,
        [EnumMember(Value = @"tractorBeam")] TRACTOR_BEAM,
        [EnumMember(Value = @"cloud")] CLOUD,
        [EnumMember(Value = @"sand")] SAND,
        [EnumMember(Value = @"plasma")] PLASMA,
        [EnumMember(Value = @"fog")] FOG,
        [EnumMember(Value = @"geyser")] GEYSER
    }
}
