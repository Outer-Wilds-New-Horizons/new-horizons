using NewHorizons.External.Modules;
using NewHorizons.External.Modules.VariableSize;
using Newtonsoft.Json;
using NewHorizons.Utility;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Internal;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// A planet or body to generate
    /// </summary>
    [JsonObject]
    public class PlanetConfig
    {
        /// <summary>
        /// Unique name of your planet
        /// </summary>
        public string Name;

        /// <summary>
        /// Version of New Horizons this config is using (Doesn't do anything)
        /// </summary>
        public string Version;

        /// <summary>
        /// Unique star system containing your planet
        /// </summary>
        [DefaultValue("SolarSystem")]
        public string StarSystem = "SolarSystem";

        /// <summary>
        /// `true` if you want to delete this planet
        /// </summary>
        public bool Destroy;
        
        /// <summary>
        /// A list of paths to child GameObjects to destroy on this planet
        /// </summary>
        public string[] RemoveChildren;
        
        /// <summary>
        /// Set to a higher number if you wish for this body to be built sooner
        /// </summary>
        [DefaultValue(-1)]
        public int BuildPriority = -1;
        
        /// <summary>
        /// Should this planet ever be shown on the title screen?
        /// </summary>
        public bool CanShowOnTitle = true;
        
        /// <summary>
        /// Does this config describe a quantum state of a custom planet defined in another file?
        /// </summary>
        public bool IsQuantumState;
        
        /// <summary>
        /// Base Properties of this Body
        /// </summary>
        public BaseModule Base;
        
        /// <summary>
        /// Describes this Body's atmosphere
        /// </summary>
        public AtmosphereModule Atmosphere;
        
        /// <summary>
        /// Describes this Body's orbit (or lack there of)
        /// </summary>
        public OrbitModule Orbit;
        
        /// <summary>
        /// Creates a ring around the planet
        /// </summary>
        public RingModule Ring;
        
        /// <summary>
        /// Generate the surface of this planet using a heightmap
        /// </summary>
        public HeightMapModule HeightMap;
        
        /// <summary>
        /// Procedural Generation
        /// </summary>
        public ProcGenModule ProcGen;
        
        /// <summary>
        /// Generate asteroids around this body
        /// </summary>
        public AsteroidBeltModule AsteroidBelt;
        
        /// <summary>
        /// Make this body a star
        /// </summary>
        public StarModule Star;
        
        /// <summary>
        /// Make this body into a focal point (barycenter)
        /// </summary>
        public FocalPointModule FocalPoint;
        
        /// <summary>
        /// Spawn various objects on this body
        /// </summary>
        public PropModule Props;
        
        /// <summary>
        /// Add ship log entries to this planet and describe how it looks in map mode
        /// </summary>
        public ShipLogModule ShipLog;
        
        /// <summary>
        /// Spawn the player at this planet
        /// </summary>
        public SpawnModule Spawn;
        
        /// <summary>
        /// Add signals that can be heard via the signal-scope to this planet
        /// </summary>
        public SignalModule Signal;
        
        /// <summary>
        /// Add a black or white hole to this planet
        /// </summary>
        public SingularityModule Singularity;
        
        /// <summary>
        /// Add lava to this planet
        /// </summary>
        public LavaModule Lava;
        
        /// <summary>
        /// Add water to this planet
        /// </summary>
        public WaterModule Water;
        
        /// <summary>
        /// Add sand to this planet
        /// </summary>
        public SandModule Sand;
        
        /// <summary>
        /// Add funnel from this planet to another
        /// </summary>
        public FunnelModule Funnel;

        #region Obsolete

        [System.Obsolete("ChildrenToDestroy is deprecated, please use RemoveChildren instead")]
        public string[] ChildrenToDestroy;

        #endregion Obsolete

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
            if (Base.CenterOfSolarSystem) Orbit.IsStatic = true;
            if (Atmosphere?.Clouds?.LightningGradient != null) Atmosphere.Clouds.HasLightning = true;

            // Backwards compatability
            // Should be the only place that obsolete things are referenced
#pragma warning disable 612, 618
            if (Base.WaterSize != 0)
            {
                Water = new WaterModule
                {
                    Size = Base.WaterSize,
                    Tint = Base.WaterTint
                };
            }

            if (Base.LavaSize != 0)
            {
                Lava = new LavaModule
                {
                    Size = Base.LavaSize
                };
            }

            if (Base.BlackHoleSize != 0)
            {
                Singularity = new SingularityModule
                {
                    Type = SingularityModule.SingularityType.BlackHole,
                    Size = Base.BlackHoleSize
                };
            }

            if (Base.IsSatellite)
            {
                Base.ShowMinimap = false;
            }

            if (ChildrenToDestroy != null)
            {
                RemoveChildren = ChildrenToDestroy;
            }

            if (Base.HasAmbientLight)
            {
                Base.AmbientLight = 0.5f;
            }

            if (Atmosphere != null)
            {
                if (!string.IsNullOrEmpty(Atmosphere.Cloud))
                {
                    Atmosphere.Clouds = new AtmosphereModule.CloudInfo()
                    {
                        OuterCloudRadius = Atmosphere.Size,
                        InnerCloudRadius = Atmosphere.Size * 0.9f,
                        Tint = Atmosphere.CloudTint,
                        TexturePath = Atmosphere.Cloud,
                        CapPath = Atmosphere.CloudCap,
                        RampPath = Atmosphere.CloudRamp,
                        FluidType = Atmosphere.FluidType,
                        UseBasicCloudShader = Atmosphere.UseBasicCloudShader,
                        Unlit = !Atmosphere.ShadowsOnClouds,
                    };
                }

                // Validate
                if (Atmosphere.Clouds?.LightningGradient != null)
                {
                    Atmosphere.Clouds.HasLightning = true;
                }

                // Former is obsolete, latter is to validate
                if (Atmosphere.HasAtmosphere || Atmosphere.AtmosphereTint != null)
                {
                    Atmosphere.UseAtmosphereShader = true;
                }
            }

            if(Props?.Tornados != null)
            {
                foreach(var tornado in Props.Tornados)
                {
                    if (tornado.downwards) tornado.type = "downwards";
                }
            }
#pragma warning restore 612, 618
        }
    }
}