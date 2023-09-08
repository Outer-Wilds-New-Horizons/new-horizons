using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules.VariableSize
{
    [JsonObject]
    public class StarModule : VariableSizeModule
    {
        /// <summary>
        /// Colour of the star at the end of its lifespan.
        /// </summary>
        public MColor endTint;

        /// <summary>
        /// Should this star explode at the end of its lifespan?
        /// </summary>
        [Obsolete("goSupernova is deprecated, please use stellarDeathType instead")] [DefaultValue(true)] public bool goSupernova = true;

        /// <summary>
        /// How long in minutes this star will last until it supernovas.
        /// </summary>
        [DefaultValue(22f)]
        public float lifespan = 22f;

        /// <summary>
        /// Should we add a star controller to this body? If you want clouds to work on a binary brown dwarf system, set this to false.
        /// </summary>
        [DefaultValue(true)] public bool hasStarController = true;

        /// <summary>
        /// The default sun has its own atmosphere that is different from regular planets. If you want that, set this to
        /// `true`.
        /// </summary>
        [DefaultValue(true)] public bool hasAtmosphere = true;

        /// <summary>
        /// Colour of the light given off. Defaults to yellowish.
        /// </summary>
        public MColor lightTint;

        /// <summary>
        /// Radius of the star.
        /// </summary>
        [DefaultValue(2000f)] [Range(0f, double.MaxValue)]
        public float size = 2000f;

        /// <summary>
        /// Relative strength of the light compared to the sun.
        /// </summary>
        [DefaultValue(1f)] [Range(0f, double.MaxValue)]
        public float solarLuminosity = 1f;

        /// <summary>
        /// Radius of the supernova. Any planets within this will be destroyed.
        /// </summary>
        [DefaultValue(50000f)]
        [Range(0f, double.MaxValue)]
        public float supernovaSize = 50000f;

        /// <summary>
        /// Speed of the supernova wall in meters per second.
        /// </summary>
        [DefaultValue(1000f)]
        [Range(1f, double.MaxValue)]
        public float supernovaSpeed = 1000f;

        /// <summary>
        /// The tint of the supernova this star creates when it dies.
        /// </summary>
        public MColor supernovaTint;

        /// <summary>
        /// Colour of the star.
        /// </summary>
        public MColor tint;

        /// <summary>
        /// Path to the texture to put as the star ramp. Optional.
        /// </summary>
        public string starRampTexture;

        /// <summary>
        /// Path to the texture to put as the star ramp while it is collapsing. Optional.
        /// </summary>
        public string starCollapseRampTexture;

        /// <summary>
        /// How far the light from the star can reach.
        /// </summary>
        [DefaultValue(50000f)] [Range(0f, double.MaxValue)]
        public float lightRadius = 50000f;

        /// <summary>
        /// The type of death your star will have.
        /// </summary>
        [DefaultValue("default")] public StellarDeathType stellarDeathType = StellarDeathType.Default;

        /// <summary>
        /// The type of stellar remnant your star will leave behind.
        /// </summary>
        [DefaultValue("default")] public StellarRemnantType stellarRemnantType = StellarRemnantType.Default;

        /// <summary>
        /// Allows overriding solar flare graphical settings.
        /// </summary>
        public SolarFlareModule solarFlareSettings;

        [JsonObject]
        public class SolarFlareModule
        {
            /// <summary>
            /// Size multiuplier for solar flares. Defaults to 1.
            /// </summary>
            [DefaultValue(1)]
            public float scaleFactor = 1f;

            /// <summary>
            /// How long a solar flare is visible for. Defaults to 15.
            /// </summary>
            [DefaultValue(15f)]
            public float lifeLength = 15f;

            /// <summary>
            /// Solar flares are emitted randomly. This is the minimum ammount of time between solar flares.
            /// </summary>
            [DefaultValue(5f)]
            public float minTimeBetweenFlares = 5f;

            /// <summary>
            /// Solar flares are emitted randomly. This is the maximum ammount of time between solar flares.
            /// </summary>
            [DefaultValue(30f)]
            public float maxTimeBetweenFlares = 30f;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StellarDeathType
    {
        [EnumMember(Value = @"default")] Default,
        [EnumMember(Value = @"none")] None,
        [EnumMember(Value = @"planetaryNebula")] PlanetaryNebula,
        [EnumMember(Value = @"supernova")] Supernova
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StellarRemnantType
    {
        [EnumMember(Value = @"default")] Default,
        [EnumMember(Value = @"whiteDwarf")] WhiteDwarf,
        [EnumMember(Value = @"neutronStar")] NeutronStar,
        [Obsolete] Pulsar,
        [EnumMember(Value = @"blackHole")] BlackHole,
        [EnumMember(Value = @"custom")] Custom
    }
}
