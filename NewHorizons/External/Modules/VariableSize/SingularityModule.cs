using NewHorizons.Utility;
using System.ComponentModel;

namespace NewHorizons.External.Modules.VariableSize
{
    public class SingularityModule : VariableSizeModule
    {
        public float Size;
        public string PairedSingularity;
        public string TargetStarSystem;
        public string Type; //BlackHole or WhiteHole
        public MVector3 Position;
        [DefaultValue(true)] public bool MakeZeroGVolume = true;
    }
}
