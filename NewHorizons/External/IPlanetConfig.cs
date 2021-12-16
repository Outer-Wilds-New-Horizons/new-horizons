using NewHorizons.Utility;

namespace NewHorizons.External
{
    public interface IPlanetConfig
    {
        string Name { get; }
        bool Destroy { get; }
        BaseModule Base {get;}
        AtmosphereModule Atmosphere { get; }
        OrbitModule Orbit { get; }
        RingModule Ring { get; }
        HeightMapModule HeightMap { get; }
        SpawnModule Spawn { get; }
    }
}
