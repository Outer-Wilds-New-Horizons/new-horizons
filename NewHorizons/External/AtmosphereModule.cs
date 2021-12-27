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
        public MColor32 CloudTint { get; set; }
        public string Cloud { get; set; }
        public string CloudCap { get; set; }
        public string CloudRamp { get; set; }
        public MColor32 FogTint { get; set; }
        public float FogDensity { get; set; }
        public float FogSize { get; set; }
        public bool HasRain { get; set; }
        public bool HasSnow { get; set; }
        public bool HasOxygen { get; set; }
        public bool HasAtmosphere { get; set; }
    }
}
