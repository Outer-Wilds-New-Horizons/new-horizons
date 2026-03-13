using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class FuelTankInfo : GeneralPropInfo
    {
        /// <summary>
        /// The type of fuel tank this is.
        /// </summary>
        [DefaultValue("hearthianTank")] public FuelTankType type = FuelTankType.HearthianTank;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum FuelTankType
        {
            [EnumMember(Value = @"hearthianTank")] HearthianTank,

            [EnumMember(Value = @"nomaiTank")] NomaiTank,

            [EnumMember(Value = @"preCrashNomaiTank")] PreCrashNomaiTank,

            [EnumMember(Value = @"dlcTorch")] DLCTorch,
        }
    }
}
