using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.SerializableData;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        /// Should the field be centered around the player or the probe.
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

        /// <summary>
        /// Ignore the particle count limit.
        /// The maximum is otherwise dependent on type. See "density" for a list.
        /// </summary>
        [DefaultValue(false)] public bool overrideParticleLimit = false;

        /// <summary>
        /// Sets the radius of the field around the player or probe. Strongly effects visual density, due to how volume works.
        /// The radius is otherwise what the base game uses for the type.
        /// </summary>
        public float? overrideFieldRadius;

        [JsonObject]
        public class HeightDensityPair
        {
            /// <summary>
            /// A specific radius
            /// </summary>
            public float height;

            /// <summary>
            /// The amount of particles at this height, within a radius given by the type or by "overrideFieldRadius"
            /// The intended/default densities (and the limits unless "overrideParticleLimit" is true):
            /// Rain: 50 (100)
            /// SnowflakesHeavy: 50 (25)
            /// SnowflakesLight: 5 (100)
            /// Embers: 50 (25)
            /// Clouds: 600 (1000)
            /// Leaves: 10 (150)
            /// Bubbles: 10 (50)
            /// Fog: 50 (250)
            /// CrystalMotes: 2 (5)
            /// RockMotes: 3 (5)
            /// IceMotes: 2 (5)
            /// SandMotes: 5 (20)
            /// Crawlies: 2 (5)
            /// Fireflies: 15 (1000)
            /// Plankton: 50 (200)
            /// Pollen: 3 (50)
            /// Current: 200 (1000)
            /// </summary>
            [Range(0f, 1000f)]public float density;
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
