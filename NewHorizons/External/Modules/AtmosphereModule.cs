using NewHorizons.Utility;
using UnityEngine;

namespace NewHorizons.External.Modules
{
    public class AtmosphereModule
    {
        public float Size { get; set; }
        public MColor AtmosphereTint { get; set; }
        public MColor FogTint { get; set; }
        public float FogDensity { get; set; }
        public float FogSize { get; set; }
        public bool HasRain { get; set; }
        public bool HasSnow { get; set; }
        public bool HasOxygen { get; set; }
        public bool UseAtmosphereShader { get; set; }
        public CloudInfo Clouds { get; set; }


        #region Obsolete
        [System.Obsolete("CloudTint is deprecated, please use CloudInfo instead")] public MColor CloudTint { get; set; }
        [System.Obsolete("CloudTint is deprecated, please use CloudInfo instead")] public string Cloud { get; set; }
        [System.Obsolete("CloudCap is deprecated, please use CloudInfo instead")] public string CloudCap { get; set; }
        [System.Obsolete("CloudRamp is deprecated, please use CloudInfo instead")] public string CloudRamp { get; set; }
        [System.Obsolete("CloudFluidType is deprecated, please use CloudInfo instead")] public string CloudFluidType { get; set; }
        [System.Obsolete("UseBasicCloudShader is deprecated, please use CloudInfo instead")] public bool UseBasicCloudShader { get; set; }
        [System.Obsolete("ShadowsOnClouds is deprecated, please use CloudInfo instead")] public bool ShadowsOnClouds { get; set; } = true;
        [System.Obsolete("HasAtmosphere is deprecated, please use UseAtmosphereShader instead")] public bool HasAtmosphere { get; set; }
        #endregion Obsolete

        public class AirInfo
        {
            public float Scale { get; set; }
            public bool HasOxygen { get; set; }
            public bool IsRaining { get; set; }
            public bool IsSnowing { get; set; }
        }

        public class CloudInfo
        {
            public float OuterCloudRadius { get; set; }
            public float InnerCloudRadius { get; set; }
            public MColor Tint { get; set; }
            public string TexturePath { get; set; }
            public string CapPath { get; set; }
            public string RampPath { get; set; }
            public string FluidType { get; set; }
            public bool UseBasicCloudShader { get; set; }
            public bool Unlit { get; set; }
            public bool HasLightning { get; set; }
            public MGradient[] LightningGradient { get; set; }
        }
    }
}
