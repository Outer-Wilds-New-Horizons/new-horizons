using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FluidType
    {
        [EnumMember(Value = @"none")] None = 0,

        [EnumMember(Value = @"water")] Water = 1,

        [EnumMember(Value = @"cloud")] Cloud = 2,

        [EnumMember(Value = @"sand")] Sand = 3,

        [EnumMember(Value = @"plasma")] Plasma = 4
    }
}
