using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.VariableSize
{
    public class VariableSizeModule : Module
    {
        public TimeValuePair[] Curve { get; set; }

        public class TimeValuePair
        {
            public float Time { get; set; }
            public float Value { get; set; }
        }
    }
}
