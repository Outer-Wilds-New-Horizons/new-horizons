using System.ComponentModel;

namespace NewHorizons.External.Modules
{
    public class AsteroidBeltModule
    {
        public float InnerRadius { get; set; }
        public float OuterRadius { get; set; }

        [DefaultValue(20)]
        public float MinSize { get; set; } = 20;

        [DefaultValue(50)]
        public float MaxSize { get; set; } = 50;

        [DefaultValue(-1)]
        public int Amount { get; set; } = -1;

        public float Inclination { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public int RandomSeed { get; set; }
        public ProcGenModule ProcGen { get; set; }
    }
}
