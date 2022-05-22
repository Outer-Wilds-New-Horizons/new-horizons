using NewHorizons.Utility;
using System.ComponentModel;

namespace NewHorizons.External.Modules.VariableSize
{
    public class FunnelModule : VariableSizeModule
    {
        public string Target { get; set; }
        
        [DefaultValue("Sand")] 
        public string Type { get; set; } = "Sand";
        
        public MColor Tint { get; set; }
    }
}
