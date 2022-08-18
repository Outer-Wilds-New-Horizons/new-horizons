using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
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
        [DefaultValue(true)] public bool goSupernova = true;

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
        /// If you want a custom stellar remnant, use this.
        /// </summary>
        public StellarRemnantModule stellarRemnant;

        public class StellarRemnantModule
        {
            /// <summary>
            /// Describes this Body's atmosphere
            /// </summary>
            public AtmosphereModule Atmosphere;

            /// <summary>
            /// Base Properties of this Body
            /// </summary>
            public BaseModule Base;

            /// <summary>
            /// Add bramble nodes to this planet and/or make this planet a bramble dimension
            /// </summary>
            public BrambleModule Bramble;

            /// <summary>
            /// Add a cloaking field to this planet
            /// </summary>
            public CloakModule Cloak;

            /// <summary>
            /// Add funnel from this planet to another
            /// </summary>
            public FunnelModule Funnel;

            /// <summary>
            /// Generate the surface of this planet using a heightmap
            /// </summary>
            public HeightMapModule HeightMap;

            /// <summary>
            /// Add lava to this planet
            /// </summary>
            public LavaModule Lava;

            /// <summary>
            /// Procedural Generation
            /// </summary>
            public ProcGenModule ProcGen;

            /// <summary>
            /// Spawn various objects on this body
            /// </summary>
            public PropModule Props;

            /// <summary>
            /// A list of paths to child GameObjects to destroy on this planet
            /// </summary>
            public string[] removeChildren;

            /// <summary>
            /// Creates a ring around the planet
            /// </summary>
            public RingModule Ring;

            /// <summary>
            /// Add sand to this planet
            /// </summary>
            public SandModule Sand;

            /// <summary>
            /// Rotation period in minutes.
            /// </summary>
            public float siderealPeriod;

            /// <summary>
            /// Make this body a star
            /// </summary>
            public StarModule Star;

            /// <summary>
            /// Add water to this planet
            /// </summary>
            public WaterModule Water;

            public PlanetConfig ConvertToPlanetConfig(PlanetConfig star)
            {
                PlanetConfig planetConfig = new PlanetConfig();
                planetConfig.name = star.name;
                planetConfig.starSystem = star.starSystem;
                planetConfig.Atmosphere = Atmosphere;
                planetConfig.Base = Base;
                planetConfig.Bramble = Bramble;
                planetConfig.Cloak = Cloak;
                planetConfig.Funnel = Funnel;
                planetConfig.HeightMap = HeightMap;
                planetConfig.Lava = Lava;
                planetConfig.Orbit = star.Orbit;
                planetConfig.ProcGen = ProcGen;
                planetConfig.Props = Props;
                planetConfig.removeChildren = removeChildren;
                planetConfig.Ring = Ring;
                planetConfig.Sand = Sand;
                planetConfig.Water = Water;
                planetConfig.Validate();
                planetConfig.Migrate();
                return planetConfig;
            }
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StellarDeathType
    {
        [EnumMember(Value = @"default")] Default,
        [EnumMember(Value = @"planetaryNebula")] PlanetaryNebula,
        [EnumMember(Value = @"supernova")] Supernova
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StellarRemnantType
    {
        [EnumMember(Value = @"default")] Default,
        [EnumMember(Value = @"whiteDwarf")] WhiteDwarf,
        [EnumMember(Value = @"neutronStar")] NeutronStar,
        [EnumMember(Value = @"pulsar")] Pulsar,
        [EnumMember(Value = @"blackHole")] BlackHole,
        [EnumMember(Value = @"custom")] Custom
    }
}
