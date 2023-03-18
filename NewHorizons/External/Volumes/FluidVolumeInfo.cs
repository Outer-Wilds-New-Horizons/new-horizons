using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Volumes
{
    [JsonObject]
    public class FluidVolumeInfo : PriorityVolumeInfo
    {
        /// <summary>
        /// Density of the fluid. The higher the density, the harder it is to go through this fluid.
        /// </summary>
        [DefaultValue(1.2f)] public float density = 1.2f;

        /// <summary>
        /// The type of fluid for this volume.
        /// </summary>
        public FluidType type;

        /// <summary>
        /// Should the player and rafts align to this fluid.
        /// </summary>
        [DefaultValue(true)] public bool alignmentFluid = true;

        /// <summary>
        /// Should the ship align to the fluid by rolling.
        /// </summary>
        public bool allowShipAutoroll;

        /// <summary>
        /// Disable this fluid volume immediately?
        /// </summary>
        public bool disableOnStart;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum FluidType
        {
            [EnumMember(Value = @"none")] NONE = 0,
            [EnumMember(Value = @"air")] AIR,
            [EnumMember(Value = @"water")] WATER,
            [EnumMember(Value = @"cloud")] CLOUD,
            [EnumMember(Value = @"sand")] SAND,
            [EnumMember(Value = @"plasma")] PLASMA,
            [EnumMember(Value = @"fog")] FOG
        }
    }
}

