using NewHorizons.External.Modules;
using NewHorizons.External.Modules.VariableSize;

namespace NewHorizons.External.Configs
{
    public class PlanetConfig
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string StarSystem { get; set; } = "SolarSystem";
        public bool Destroy { get; set; }
        public string[] ChildrenToDestroy { get; set; }
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

        public PlanetConfig()
        {
            // Always have to have a base module
            if (Base == null) Base = new BaseModule();
            if (Orbit == null) Orbit = new OrbitModule();
            if (ShipLog == null) ShipLog = new ShipLogModule();
        }

        public void Validate()
        {
            if (Base.CenterOfSolarSystem) Orbit.IsStatic = true;

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

            if(Base.BlackHoleSize != 0)
            {
                Singularity = new SingularityModule();
                Singularity.Type = "BlackHole";
                Singularity.Size = Base.BlackHoleSize;
            }

            if(Base.IsSatellite)
            {
                Base.ShowMinimap = false;
            }
        }
    }
}
