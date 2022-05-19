using NewHorizons.Utility;
namespace NewHorizons.External.Modules
{
    public class AtmosphereModule
    {
        public float Size { get; set; }
        public MColor CloudTint { get; set; }
        public string Cloud { get; set; }
        public string CloudCap { get; set; }
        public string CloudRamp { get; set; }
        public string CloudFluidType { get; set; }
        public bool UseBasicCloudShader { get; set; }
        public bool ShadowsOnClouds { get; set; } = true;
        public MColor FogTint { get; set; }
        public float FogDensity { get; set; }
        public float FogSize { get; set; }
        public bool HasRain { get; set; }
        public bool HasSnow { get; set; }
        public bool HasOxygen { get; set; }
        public bool HasAtmosphere { get; set; }
        public MColor AtmosphereTint { get; set; }

        public class AirInfo
        {
            public float Scale { get; set; }
            public bool HasOxygen { get; set; }
            public bool IsRaining { get; set; }
            public bool IsSnowing { get; set; }
        }
    }
}
