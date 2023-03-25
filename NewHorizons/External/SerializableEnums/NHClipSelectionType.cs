using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NHClipSelectionType
    {
        [EnumMember(Value = @"random")] RANDOM,
        [EnumMember(Value = @"sequential")] SEQUENTIAL,
        [EnumMember(Value = @"manual")] MANUAL
    }
}
