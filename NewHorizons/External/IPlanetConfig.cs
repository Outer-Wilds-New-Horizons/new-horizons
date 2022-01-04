using NewHorizons.Utility;

namespace NewHorizons.External
{
    public interface IPlanetConfig
    {
        string Name { get; }
        string StarSystem { get; } 
        bool Destroy { get; }
        int BuildPriority { get; }
        BaseModule Base {get;}
        AtmosphereModule Atmosphere { get; }
        OrbitModule Orbit { get; }
        RingModule Ring { get; }
        HeightMapModule HeightMap { get; }
        ProcGenModule ProcGen { get; }
        AsteroidBeltModule AsteroidBelt { get; }
        StarModule Star { get; }
        FocalPointModule FocalPoint { get; }
        PropModule Props { get; }
        SpawnModule Spawn { get; }
        SignalModule Signal { get; }
        SingularityModule Singularity { get; }
    }
}
