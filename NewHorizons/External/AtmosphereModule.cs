using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class AtmosphereModule : Module
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
    }
}
