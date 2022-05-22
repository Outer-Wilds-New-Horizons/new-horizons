using NewHorizons.Utility;
namespace NewHorizons.External.Modules.VariableSize
{
    public class SingularityModule : VariableSizeModule
    {
        public float Size;
        public string PairedSingularity;
        public string TargetStarSystem;
        public string Type; //BlackHole or WhiteHole
        public MVector3 Position;
        public bool MakeZeroGVolume = true;
    }
}
