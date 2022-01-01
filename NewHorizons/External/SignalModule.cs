using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class SignalModule : Module
    {
        public SignalInfo[] Signals;

        public class SignalInfo
        {
            public MVector3 Position;
            public string Frequency;
            public string Name;
            public string AudioClip;
            public float SourceRadius = 1f;
            public bool OnlyAudibleToScope = true;
        }
    }
}
