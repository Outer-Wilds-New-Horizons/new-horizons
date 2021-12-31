using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class StarModule : Module
    {
        public float Size { get; set; } = 2000f;
        public MColor32 Tint { get; set; } 
        public MColor32 SolarFlareTint { get; set; }
        public MColor32 LightTint { get; set; }
        public float SolarLuminosity { get; set; } = 1f;
    }
}
