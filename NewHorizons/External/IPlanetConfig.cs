using NewHorizons.Utility;

namespace NewHorizons.External
{
    public interface IPlanetConfig
    {
        string Name { get; }
        int SemiMajorAxis { get; }
        int Inclination { get; }
        string PrimaryBody { get; }
        bool IsMoon { get; }
        float AtmoEndSize { get; }
        bool HasClouds { get; }
        float TopCloudSize { get; }
        float BottomCloudSize { get; }
        MColor32 TopCloudTint { get; }
        MColor32 BottomCloudTint { get; }
        bool HasWater { get; }
        float WaterSize { get; }
        bool HasRain { get; }
        bool HasGravity { get; }
        float SurfaceAcceleration { get; }
        bool HasMapMarker { get; }
        bool HasFog { get; }
        MColor32 FogTint { get; }
        float FogDensity { get; }
        float FogSize { get; }
        bool HasGround { get; }
        float GroundSize { get; }
        bool IsTidallyLocked { get; }
        MColor32 LightTint { get; }
        bool HasSnow { get; }
        float LongitudeOfAscendingNode { get; }
        float Eccentricity { get; }
        float ArgumentOfPeriapsis { get; }
        bool HasRings { get; }
        float RingInnerRadius { get; }
        float RingOuterRadius { get; }
        float RingInclination { get; }
        float RingLongitudeOfAscendingNode { get; }
        string RingTexture { get; }
        bool HasBlackHole { get; }
        bool HasLava { get; }
        float LavaSize { get; }
        bool Destroy { get; }
    }
}
