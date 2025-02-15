using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.SerializableData;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class ParticleFieldModule
    {
        /// <summary>
        /// Particle type for this vection field.
        /// </summary>
        public ParticleFieldType type;

        /// <summary>
        /// What the particle field activates based on.
        /// </summary>
        [DefaultValue("player")] public FollowTarget followTarget = FollowTarget.Player;

        /// <summary>
        /// Density by height curve. Determines how many particles are emitted at different heights.
        /// Defaults to a curve based on minimum and maximum heights of various other modules.
        /// </summary>
        public HeightDensityPair[] densityByHeightCurve;

        /// <summary>
        /// An optional rename of this object
        /// </summary>
        public string rename;

        [JsonObject]
        public class HeightDensityPair
        {
            /// <summary>
            /// A specific radius
            /// </summary>
            public float height;

            /// <summary>
            /// The particle count for this radius.
            /// </summary>
            public float density;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum ParticleFieldType
        {
            [EnumMember(Value = @"rain")] Rain,
            [EnumMember(Value = @"snowflakesHeavy")] SnowflakesHeavy,
            [EnumMember(Value = @"snowflakesLight")] SnowflakesLight,
            [EnumMember(Value = @"embers")] Embers,
            [EnumMember(Value = @"clouds")] Clouds,
            [EnumMember(Value = @"leaves")] Leaves,
            [EnumMember(Value = @"bubbles")] Bubbles,
            [EnumMember(Value = @"fog")] Fog,
            [EnumMember(Value = @"crystalMotes")] CrystalMotes,
            [EnumMember(Value = @"rockMotes")] RockMotes,
            [EnumMember(Value = @"iceMotes")] IceMotes,
            [EnumMember(Value = @"sandMotes")] SandMotes,
            [EnumMember(Value = @"crawlies")] Crawlies,
            [EnumMember(Value = @"fireflies")] Fireflies,
            [EnumMember(Value = @"plankton")] Plankton,
            [EnumMember(Value = @"pollen")] Pollen,
            [EnumMember(Value = @"current")] Current
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum FollowTarget
        {
            [EnumMember(Value = @"player")] Player,
            [EnumMember(Value = @"probe")] Probe
        }
    }
}
