using NewHorizons.Utility;
using System;
using System.Collections.Generic;

namespace NewHorizons.External
{
    public class PlanetConfig : IPlanetConfig
    {
        public string Name { get; set; }
        public int SemiMajorAxis { get; set; }
        public int Inclination { get; set; }
        public string PrimaryBody { get; set; }
        public bool IsMoon { get; set; }
        public float AtmoEndSize { get; set; }
        public bool HasClouds { get; set; }
        public float TopCloudSize { get; set; }
        public float BottomCloudSize { get; set; }
        public MColor32 TopCloudTint { get; set; }
        public MColor32 BottomCloudTint { get; set; }
        public bool HasWater { get; set; }
        public float WaterSize { get; set; }
        public bool HasRain { get; set; }
        public bool HasGravity { get; set; }
        public float SurfaceAcceleration { get; set; }
        public bool HasMapMarker { get; set; }
        public bool HasFog { get; set; }
        public MColor32 FogTint { get; set; }
        public float FogDensity { get; set; }
        public float FogSize { get; set; }
        public bool HasGround { get; set; }
        public float GroundSize { get; set; }
        public bool IsTidallyLocked { get; set; }
        public MColor32 LightTint { get; set; }
        public bool HasSnow { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public float Eccentricity { get; set; }
        public float ArgumentOfPeriapsis { get; set; }
        public bool HasRings { get; set; }
        public float RingInnerRadius { get; set; }
        public float RingOuterRadius { get; set; }
        public float RingInclination { get; set; }
        public float RingLongitudeOfAscendingNode { get; set; }
        public string RingTexture { get; set; }
        public bool HasBlackHole { get; set; }
        public bool HasLava { get; set; }
        public float LavaSize { get; set; }
        public bool Destroy { get; set; }

        public PlanetConfig(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return;
            }
            foreach (var item in dict)
            {
                Logger.Log($"{item.Key} : {item.Value}", Logger.LogType.Log);
                var field = GetType().GetField(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, Convert.ChangeType(item.Value, field.FieldType));
            }
        }
    }
}
