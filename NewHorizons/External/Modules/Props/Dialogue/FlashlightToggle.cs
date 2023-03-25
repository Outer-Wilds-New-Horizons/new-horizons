using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Props.Dialogue
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FlashlightToggle
    {
        [EnumMember(Value = @"none")] None = -1,
        [EnumMember(Value = @"turnOff")] TurnOff = 0,
        [EnumMember(Value = @"turnOffThenOn")] TurnOffThenOn = 1,
    }
}
