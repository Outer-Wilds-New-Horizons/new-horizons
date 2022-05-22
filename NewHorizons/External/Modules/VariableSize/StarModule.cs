using NewHorizons.Utility;
using System.ComponentModel;

namespace NewHorizons.External.Modules.VariableSize
{
    public class StarModule : VariableSizeModule
    {
        [DefaultValue(2000)] 
        public float Size { get; set; } = 2000f;

        public MColor Tint { get; set; } 

        public MColor EndTint { get; set; } 

        public MColor SupernovaTint { get; set; }

        public MColor LightTint { get; set; }

        [DefaultValue(1f)] 
        public float SolarLuminosity { get; set; } = 1f;

        [DefaultValue(true)] 
        public bool HasAtmosphere { get; set; } = true;

        [DefaultValue(true)] 
        public bool GoSupernova { get; set; } = true;
    }
}
