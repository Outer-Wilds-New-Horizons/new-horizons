namespace NewHorizons.Components
{
    public class NHInnerFogWarpVolume : InnerFogWarpVolume
    {
        public override bool IsProbeOnly() => _exitRadius <= 6;
        public override float GetFogThickness() => _exitRadius;
    }
}
