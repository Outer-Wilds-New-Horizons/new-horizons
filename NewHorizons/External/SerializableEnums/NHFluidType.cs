using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NHFluidType
    {
        [EnumMember(Value = @"none")] NONE = 0,

        [EnumMember(Value = @"water")] WATER = 1,

        [EnumMember(Value = @"cloud")] CLOUD = 2,

        [EnumMember(Value = @"sand")] SAND = 3,

        [EnumMember(Value = @"plasma")] PLASMA = 4
    }
}
