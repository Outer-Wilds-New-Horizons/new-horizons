using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Volumes
{
    [JsonObject]
    public class DestructionVolumeInfo : VanishVolumeInfo
    {
        /// <summary>
        /// The type of death the player will have if they enter this volume.
        /// </summary>
        [DefaultValue("default")] public DeathType deathType = DeathType.Default;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum DeathType
        {
            [EnumMember(Value = @"default")] Default,
            [EnumMember(Value = @"impact")] Impact,
            [EnumMember(Value = @"asphyxiation")] Asphyxiation,
            [EnumMember(Value = @"energy")] Energy,
            [EnumMember(Value = @"supernova")] Supernova,
            [EnumMember(Value = @"digestion")] Digestion,
            [EnumMember(Value = @"bigBang")] BigBang,
            [EnumMember(Value = @"crushed")] Crushed,
            [EnumMember(Value = @"meditation")] Meditation,
            [EnumMember(Value = @"timeLoop")] TimeLoop,
            [EnumMember(Value = @"lava")] Lava,
            [EnumMember(Value = @"blackHole")] BlackHole,
            [EnumMember(Value = @"dream")] Dream,
            [EnumMember(Value = @"dreamExplosion")] DreamExplosion,
            [EnumMember(Value = @"crushedByElevator")] CrushedByElevator
        }
    }
}

