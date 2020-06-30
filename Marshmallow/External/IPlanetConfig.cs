using Marshmallow.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.External
{
    public interface IPlanetConfig
    {
        string Name { get; }
        MVector3 Position { get; }
        int OrbitAngle { get; }
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
        bool HasGround { get; }
        float GroundSize { get; }
        bool IsTidallyLocked { get; }
        MColor32 LightTint { get; }
    }
}
