using NewHorizons.External;
using NewHorizons.Utility.CommonResources;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Components.Orbital
{
    public class NHAstroObject : AstroObject, IKeplerCoordinates
    {
        public float Inclination { get; set; }
        public int SemiMajorAxis { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public float Eccentricity { get; set; }
        public float ArgumentOfPeriapsis { get; set; }
        public float TrueAnomaly { get; set; }
        public bool HideDisplayName { get; set; }

        public void SetKeplerCoordinatesFromOrbitModule(OrbitModule orbit)
        {
            var keplerCoordinates = KeplerCoordinates.fromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
            Inclination = keplerCoordinates.inclinationAngle;
            SemiMajorAxis = (int)keplerCoordinates.semiMajorRadius;
            LongitudeOfAscendingNode = keplerCoordinates.ascendingAngle;
            Eccentricity = keplerCoordinates.eccentricity;
            ArgumentOfPeriapsis = keplerCoordinates.periapseAngle;
            TrueAnomaly = keplerCoordinates.trueAnomaly;
        }

        public void SetKeplerCoordinatesFromTrueAnomaly(float ecc, float a, float i, float p, float l, float trueAnomaly)
        {
            var keplerCoordinates = KeplerCoordinates.fromTrueAnomaly(ecc, a, i, p, l, trueAnomaly);
            Inclination = keplerCoordinates.inclinationAngle;
            SemiMajorAxis = (int)keplerCoordinates.semiMajorRadius;
            LongitudeOfAscendingNode = keplerCoordinates.ascendingAngle;
            Eccentricity = keplerCoordinates.eccentricity;
            ArgumentOfPeriapsis = keplerCoordinates.periapseAngle;
            TrueAnomaly = keplerCoordinates.trueAnomaly;
        }

        public override string ToString()
        {
            return $"ParameterizedAstroObject: Eccentricity {Eccentricity}, SemiMajorAxis {SemiMajorAxis}, Inclination {Inclination}, ArgumentOfPeriapsis {ArgumentOfPeriapsis}, LongitudeOfAscendingNode {LongitudeOfAscendingNode}, TrueAnomaly {TrueAnomaly}";
        }

        public KeplerCoordinates GetKeplerCoords()
        {
            return KeplerCoordinates.fromTrueAnomaly(Eccentricity, SemiMajorAxis, Inclination, ArgumentOfPeriapsis, LongitudeOfAscendingNode, TrueAnomaly);
        }
    }
}
