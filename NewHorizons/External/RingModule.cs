using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class RingModule : Module
    {
        public float InnerRadius { get; set; }
        public float OuterRadius { get; set; }
        public float Inclination { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public string Texture { get; set; }
    }
}
