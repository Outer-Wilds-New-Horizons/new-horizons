using NewHorizons.Components.Orbital;
using NewHorizons.Utility;
using System.ComponentModel;

namespace NewHorizons.External.Modules
{
    public class OrbitModule : IOrbitalParameters
    {
        public float SemiMajorAxis { get; set; }
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
        public MVector3 AlignmentAxis { get; set; }

        [DefaultValue(true)] 
        public bool ShowOrbitLine { get; set; } = true;

        public bool DottedOrbitLine { get; set; }
        public bool IsStatic { get; set; }
        public MColor Tint { get; set; }
        public bool TrackingOrbitLine { get; set; }

        public OrbitalParameters GetOrbitalParameters(Gravity primaryGravity, Gravity secondaryGravity)
        {
            return OrbitalParameters.FromTrueAnomaly(primaryGravity, secondaryGravity, Eccentricity, SemiMajorAxis, Inclination, ArgumentOfPeriapsis, LongitudeOfAscendingNode, TrueAnomaly);
        }
    }
}
