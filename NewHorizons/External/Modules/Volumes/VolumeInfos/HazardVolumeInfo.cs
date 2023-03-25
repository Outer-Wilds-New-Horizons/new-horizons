using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{

    [JsonObject]
    public class HazardVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The type of hazard for this volume.
        /// </summary>
        [DefaultValue("general")] public HazardType type = HazardType.GENERAL;

        /// <summary>
        /// The amount of damage you will take per second while inside this volume.
        /// </summary>
        [DefaultValue(10f)] public float damagePerSecond = 10f;

        /// <summary>
        /// The type of damage you will take when you first touch this volume.
        /// </summary>
        [DefaultValue("impact")] public InstantDamageType firstContactDamageType = InstantDamageType.Impact;

        /// <summary>
        /// The amount of damage you will take when you first touch this volume.
        /// </summary>
        public float firstContactDamage;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum HazardType
        {
            [EnumMember(Value = @"none")] NONE = 0,
            [EnumMember(Value = @"general")] GENERAL = 1,
            [EnumMember(Value = @"ghostMatter")] DARKMATTER = 2,
            [EnumMember(Value = @"heat")] HEAT = 4,
            [EnumMember(Value = @"fire")] FIRE = 8,
            [EnumMember(Value = @"sandfall")] SANDFALL = 16,
            [EnumMember(Value = @"electricity")] ELECTRICITY = 32,
            [EnumMember(Value = @"rapids")] RAPIDS = 64,
            [EnumMember(Value = @"riverHeat")] RIVERHEAT = 128,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum InstantDamageType
        {
            [EnumMember(Value = @"impact")] Impact,
            [EnumMember(Value = @"puncture")] Puncture,
            [EnumMember(Value = @"electrical")] Electrical
        }
    }
}
