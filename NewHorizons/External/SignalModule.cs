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
            public string AudioClip = null;
            public string AudioFilePath = null;
            public float SourceRadius = 1f;
            public float DetectionRadius = 0f;
            public float IdentificationRadius = 10f;
            public bool OnlyAudibleToScope = true;
            public bool InsideCloak = false;
        }
    }
}
