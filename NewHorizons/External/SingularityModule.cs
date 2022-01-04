using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class SingularityModule : Module
    {
        public float Size;
        public string PairedSingularity;
        public string TargetStarSystem;
        public string Type; //BlackHole or WhiteHole
        public MVector3 Position;
    }
}
