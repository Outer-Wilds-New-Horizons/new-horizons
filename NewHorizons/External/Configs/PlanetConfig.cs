using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.VariableSize;
using Newtonsoft.Json;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// Describes a body to generate
    /// </summary>
    [JsonObject(Title = "Celestial Body")]
    public class PlanetConfig
    {
        /// <summary>
        /// Generate asteroids around this body
        /// </summary>
        public AsteroidBeltModule AsteroidBelt;

        /// <summary>
        /// Describes this Body's atmosphere
        /// </summary>
        public AtmosphereModule Atmosphere;

        /// <summary>
        /// Base Properties of this Body
        /// </summary>
        public BaseModule Base;

        /// <summary>
        /// Set to a higher number if you wish for this body to be built sooner
        /// </summary>
        [DefaultValue(-1)] public int buildPriority = -1;

        /// <summary>
        /// Should this planet ever be shown on the title screen?
        /// </summary>
        public bool canShowOnTitle = true;

        #region Obsolete

        [Obsolete("ChildrenToDestroy is deprecated, please use RemoveChildren instead")]
        public string[] childrenToDestroy;

        #endregion Obsolete

        /// <summary>
        /// `true` if you want to delete this planet
        /// </summary>
        public bool destroy;

        /// <summary>
        /// Make this body into a focal point (barycenter)
        /// </summary>
        public FocalPointModule FocalPoint;

        /// <summary>
        /// Add funnel from this planet to another
        /// </summary>
        public FunnelModule Funnel;

        /// <summary>
        /// Generate the surface of this planet using a heightmap
        /// </summary>
        public HeightMapModule HeightMap;

        /// <summary>
        /// Does this config describe a quantum state of a custom planet defined in another file?
        /// </summary>
        public bool isQuantumState;

        /// <summary>
        /// Add lava to this planet
        /// </summary>
        public LavaModule Lava;

        /// <summary>
        /// Unique name of your planet
        /// </summary>
        [Required]
        public string name;

        /// <summary>
        /// Describes this Body's orbit (or lack there of)
        /// </summary>
        public OrbitModule Orbit;

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
        /// Add ship log entries to this planet and describe how it looks in map mode
        /// </summary>
        public ShipLogModule ShipLog;

        /// <summary>
        /// Add signals that can be heard via the signal-scope to this planet
        /// </summary>
        public SignalModule Signal;

        /// <summary>
        /// Add a black or white hole to this planet
        /// </summary>
        public SingularityModule Singularity;

        /// <summary>
        /// Spawn the player at this planet
        /// </summary>
        public SpawnModule Spawn;

        /// <summary>
        /// Make this body a star
        /// </summary>
        public StarModule Star;

        /// <summary>
        /// Unique star system containing your planet
        /// </summary>
        [DefaultValue("SolarSystem")] public string starSystem = "SolarSystem";

        /// <summary>
        /// Version of New Horizons this config is using (Doesn't do anything)
        /// </summary>
        public string version;

        /// <summary>
        /// Add water to this planet
        /// </summary>
        public WaterModule Water;

        public PlanetConfig()
        {
            // Always have to have a base module
            if (Base == null) Base = new BaseModule();
            if (Orbit == null) Orbit = new OrbitModule();
            if (ShipLog == null) ShipLog = new ShipLogModule();
        }

        public void MigrateAndValidate()
        {
            // Validate
            if (Base.centerOfSolarSystem) Orbit.isStatic = true;
            if (Atmosphere?.clouds?.lightningGradient != null) Atmosphere.clouds.hasLightning = true;

            // Backwards compatability
            // Should be the only place that obsolete things are referenced
#pragma warning disable 612, 618
            if (Base.waterSize != 0)
                Water = new WaterModule
                {
                    size = Base.waterSize,
                    tint = Base.waterTint
                };

            if (Base.lavaSize != 0)
                Lava = new LavaModule
                {
                    size = Base.lavaSize
                };

            if (Base.blackHoleSize != 0)
                Singularity = new SingularityModule
                {
                    type = SingularityModule.SingularityType.BlackHole,
                    size = Base.blackHoleSize
                };

            if (Base.isSatellite) Base.showMinimap = false;

            if (childrenToDestroy != null) removeChildren = childrenToDestroy;

            if (Base.hasAmbientLight) Base.ambientLight = 0.5f;

            if (Atmosphere != null)
            {
                if (!string.IsNullOrEmpty(Atmosphere.cloud))
                    Atmosphere.clouds = new AtmosphereModule.CloudInfo
                    {
                        outerCloudRadius = Atmosphere.size,
                        innerCloudRadius = Atmosphere.size * 0.9f,
                        tint = Atmosphere.cloudTint,
                        texturePath = Atmosphere.cloud,
                        capPath = Atmosphere.cloudCap,
                        rampPath = Atmosphere.cloudRamp,
                        fluidType = Atmosphere.fluidType,
                        useBasicCloudShader = Atmosphere.useBasicCloudShader,
                        unlit = !Atmosphere.shadowsOnClouds
                    };

                // Validate
                if (Atmosphere.clouds?.lightningGradient != null) Atmosphere.clouds.hasLightning = true;

                // Former is obsolete, latter is to validate
                if (Atmosphere.hasAtmosphere || Atmosphere.atmosphereTint != null)
                    Atmosphere.useAtmosphereShader = true;

                // useBasicCloudShader is obsolete
                if (Atmosphere.clouds != null && Atmosphere.clouds.useBasicCloudShader) 
                    Atmosphere.clouds.cloudsPrefab = CloudPrefabType.Basic;
            }

            if (Props?.tornados != null)
                foreach (var tornado in Props.tornados)
                    if (tornado.downwards)
                        tornado.type = PropModule.TornadoInfo.TornadoType.Downwards;
        }
    }
}