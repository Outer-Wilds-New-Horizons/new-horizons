using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class AsteroidBeltModule : Module
    {
        public float InnerRadius { get; set; }
        public float OuterRadius { get; set; }
        public float MinSize { get; set; } = 20;
        public float MaxSize { get; set; } = 50;
        public int Amount { get; set; } = -1;
        public float Inclination { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public int RandomSeed { get; set; }
        public ProcGenModule ProcGen { get; set; }
    }
}
