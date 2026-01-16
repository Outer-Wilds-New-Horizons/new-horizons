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
            /// The intended/default densities:
            /// Rain: 50
            /// SnowflakesHeavy: 50
            /// SnowflakesLight: 5
            /// Embers: 25
            /// Clouds: 600
            /// Leaves: 10
            /// Bubbles: 10
            /// Fog: 50
            /// CrystalMotes: 2
            /// RockMotes: 3
            /// IceMotes: 2
            /// SandMotes: 5
            /// Crawlies: 2
            /// Fireflies: 15
            /// Plankton: 50
            /// Pollen: 3
            /// Current: 200
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
