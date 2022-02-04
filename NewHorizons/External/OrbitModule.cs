using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class OrbitModule : Module
    {
        public int SemiMajorAxis { get; set; } = 5000;
        public float Inclination { get; set; }
        public string PrimaryBody { get; set; }
        public bool IsMoon { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public float Eccentricity { get; set; }
        public float ArgumentOfPeriapsis { get; set; }
        public float TrueAnomaly { get; set; }
        public float AxialTilt { get; set; }
        public float SiderealPeriod { get; set; }
        public bool IsTidallyLocked { get; set; }
        public bool ShowOrbitLine { get; set; } = true;
        public bool IsStatic { get; set; }
        public MColor Tint { get; set; }
    }
}
