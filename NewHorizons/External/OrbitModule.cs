using NewHorizons.Utility;
using NewHorizons.Utility.CommonResources;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class OrbitModule : Module, IKeplerCoordinates
    {
        public int SemiMajorAxis { get; set; }
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
        public bool ShowOrbitLine { get; set; } = true;
        public bool IsStatic { get; set; }
        public MColor Tint { get; set; }
        public bool TrackingOrbitLine { get; set; } = false;

        public KeplerCoordinates GetKeplerCoords()
        {
            return KeplerCoordinates.fromTrueAnomaly(Eccentricity, SemiMajorAxis, Inclination, ArgumentOfPeriapsis, LongitudeOfAscendingNode, TrueAnomaly);
        }
    }
}
