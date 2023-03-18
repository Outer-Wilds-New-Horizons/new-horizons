using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Volumes
{
    [JsonObject]
    public class LoadCreditsVolumeInfo : VolumeInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CreditsType
        {
            [EnumMember(Value = @"fast")] Fast = 0,

            [EnumMember(Value = @"final")] Final = 1,

            [EnumMember(Value = @"kazoo")] Kazoo = 2
        }

        [DefaultValue("fast")]
        public CreditsType creditsType = CreditsType.Fast;
    }
}

