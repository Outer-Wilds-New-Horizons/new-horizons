using NewHorizons.Utility;
using System.ComponentModel;

namespace NewHorizons.External.Modules
{
    public class SignalModule 
    {
        public SignalInfo[] Signals;

        public class SignalInfo
        {
            public MVector3 Position;
            public string Frequency;
            public string Name;
            public string AudioClip;
            public string AudioFilePath;

            [DefaultValue("")]
            public string Reveals = "";

            [DefaultValue(1f)]
            public float SourceRadius = 1f;
            public float DetectionRadius;

            [DefaultValue(10f)]
            public float IdentificationRadius = 10f;

            [DefaultValue(true)]
            public bool OnlyAudibleToScope = true;

            public bool InsideCloak;
        }
    }
}
