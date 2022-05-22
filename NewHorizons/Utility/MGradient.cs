using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Utility
{
    public class MGradient
    {
        public MGradient(float time, MColor tint)
        {
            Time = time;
            Tint = tint;
        }

        public float Time { get; }
        public MColor Tint { get; }
    }
}
