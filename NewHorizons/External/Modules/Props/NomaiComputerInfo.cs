using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class NomaiComputerInfo : GeneralPropInfo
    {
        /// <summary>
        /// What design the computer will use.
        /// </summary>
        [DefaultValue(NomaiComputerType.NORMAL)] public NomaiComputerType type = NomaiComputerType.NORMAL;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum NomaiComputerType
    {
        [EnumMember(Value = @"normal")] NORMAL = 0,
        [EnumMember(Value = @"precrash")] PRECRASH = 1
    }
}
