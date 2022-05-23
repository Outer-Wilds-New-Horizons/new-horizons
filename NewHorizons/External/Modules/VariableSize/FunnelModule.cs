using System.Runtime.Serialization;
using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules.VariableSize
{
    public enum FunnelType
    {
        [EnumMember(Value = @"Sand")] Sand = 0,

        [EnumMember(Value = @"Water")] Water = 1,

        [EnumMember(Value = @"Lava")] Lava = 2,

        [EnumMember(Value = @"Star")] Star = 3
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
        [JsonConverter(typeof(StringEnumConverter))]
        public FunnelType type = FunnelType.Sand;
    }
}