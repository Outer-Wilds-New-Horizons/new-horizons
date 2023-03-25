using System.ComponentModel;
using System.Runtime.Serialization;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FunnelType
    {
        [EnumMember(Value = @"sand")] Sand = 0,

        [EnumMember(Value = @"water")] Water = 1,

        [EnumMember(Value = @"lava")] Lava = 2,

        [EnumMember(Value = @"star")] Star = 3
    }

    [JsonObject]
    public class FunnelModule : VariableSizeModule
    {
        /// <summary>
        /// The planet the funnel will flow to
        /// </summary>
        public string target;

        /// <summary>
        /// Tint of the funnel
        /// </summary>
        public MColor tint;

        /// <summary>
        /// Type of fluid the funnel transfers
        /// </summary>
        [DefaultValue("sand")] public FunnelType type = FunnelType.Sand;
    }
}