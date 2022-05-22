namespace NewHorizons.External.Modules.VariableSize
{
    public class RingModule : VariableSizeModule
    {
        public float InnerRadius { get; set; }
        public float OuterRadius { get; set; }
        public float Inclination { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public string Texture { get; set; }
        public bool Unlit { get; set; }
        public float RotationSpeed { get; set; } 
        public string FluidType { get; set; }
    }
}
