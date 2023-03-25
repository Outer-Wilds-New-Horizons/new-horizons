using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CreditsType
    {
        [EnumMember(Value = @"fast")] Fast = 0,

        [EnumMember(Value = @"final")] Final = 1,

        [EnumMember(Value = @"kazoo")] Kazoo = 2
    }
}
