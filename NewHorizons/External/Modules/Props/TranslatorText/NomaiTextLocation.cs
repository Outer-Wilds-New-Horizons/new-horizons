using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.TranslatorText
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NomaiTextLocation
    {
        [EnumMember(Value = @"unspecified")] UNSPECIFIED = 0,

        [EnumMember(Value = @"a")] A = 1,

        [EnumMember(Value = @"b")] B = 2
    }
}
