using NewHorizons.External.Modules;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using UnityEngine;

namespace NewHorizons.External.Configs
{
    public class PlanetConfig
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string StarSystem { get; set; } = "SolarSystem";
        public bool Destroy { get; set; }
        public string[] RemoveChildren { get; set; }
        public int BuildPriority { get; set; } = -1;
        public bool CanShowOnTitle { get; set; } = true;
        public bool IsQuantumState { get; set; }
        public BaseModule Base { get; set; }
        public AtmosphereModule Atmosphere { get; set; }
        public OrbitModule Orbit { get; set; }
        public RingModule Ring { get; set; }
        public HeightMapModule HeightMap { get; set; }
        public ProcGenModule ProcGen { get; set; }
        public AsteroidBeltModule AsteroidBelt { get; set; }
        public StarModule Star { get; set; }
        public FocalPointModule FocalPoint { get; set; }
        public PropModule Props { get; set; }
        public ShipLogModule ShipLog { get; set; }
        public SpawnModule Spawn { get; set; }
        public SignalModule Signal { get; set; }
        public SingularityModule Singularity { get; set; }
        public LavaModule Lava { get; set; }
        public WaterModule Water { get; set; }
        public SandModule Sand { get; set; }
        public FunnelModule Funnel { get; set; }

        #region Obsolete
        [System.Obsolete("ChildrenToDestroy is deprecated, please use RemoveChildren instead")] public string[] ChildrenToDestroy { get; set; }
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
                Water = new WaterModule();
                Water.Size = Base.WaterSize;
                Water.Tint = Base.WaterTint;
            }

            if (Base.LavaSize != 0)
            {
                Lava = new LavaModule();
                Lava.Size = Base.LavaSize;
            }

            if (Base.BlackHoleSize != 0)
            {
                Singularity = new SingularityModule();
                Singularity.Type = "BlackHole";
                Singularity.Size = Base.BlackHoleSize;
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
                        FluidType = Atmosphere.CloudFluidType,
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
#pragma warning restore 612, 618
        }
    }
}
