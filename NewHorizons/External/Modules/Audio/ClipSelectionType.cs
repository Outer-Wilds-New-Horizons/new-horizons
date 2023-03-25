using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Audio
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ClipSelectionType
    {
        [EnumMember(Value = @"random")] RANDOM,
        [EnumMember(Value = @"sequential")] SEQUENTIAL,
        [EnumMember(Value = @"manual")] MANUAL
    }
}
