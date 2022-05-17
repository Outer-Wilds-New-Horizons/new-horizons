using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.VariableSize
{
    public class StarModule : VariableSizeModule
    {
        public float Size { get; set; } = 2000f;
        public MColor Tint { get; set; } 
        public MColor EndTint { get; set; } 
        public MColor SupernovaTint { get; set; }
        public MColor SolarFlareTint { get; set; }
        public MColor LightTint { get; set; }
        public float SolarLuminosity { get; set; } = 1f;
        public bool HasAtmosphere { get; set; } = true;
        public bool GoSupernova { get; set; } = true;
    }
}
