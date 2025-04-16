using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NHCreditsType
    {
        [EnumMember(Value = @"fast")] Fast = 0,

        [EnumMember(Value = @"final")] Final = 1,

        [EnumMember(Value = @"kazoo")] Kazoo = 2,

        [EnumMember(Value = @"custom")] Custom = 3,

        [EnumMember(Value = @"none")] None = 4
    }
}
