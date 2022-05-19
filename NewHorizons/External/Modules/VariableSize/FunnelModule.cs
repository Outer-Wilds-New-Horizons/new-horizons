using NewHorizons.Utility;
namespace NewHorizons.External.Modules.VariableSize
{
    public class FunnelModule : VariableSizeModule
    {
        public string Target { get; set; }
        public string Type { get; set; } = "Sand";
        public MColor Tint { get; set; }
    }
}
