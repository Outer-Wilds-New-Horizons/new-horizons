using NewHorizons.External;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.OrbitalPhysics
{
    public class ParameterizedAstroObject : AstroObject
    {
        private KeplerCoordinates _keplerCoordinates;

        public float Eccentricity
        {
            get { return _keplerCoordinates.eccentricity; }
        }

        public float SemiMajorAxis
        {
            get { return _keplerCoordinates.semiMajorRadius; }
        }

        public float Inclination
        {
            get { return _keplerCoordinates.inclinationAngle - 90; }
        }

        public float ArgumentOfPeriapsis
        {
            get { return _keplerCoordinates.periapseAngle; }
        }

        public float LongitudeOfAscendingNode
        {
            get { return _keplerCoordinates.ascendingAngle; }
        }

        public float TrueAnomaly
        {
            get { return _keplerCoordinates.trueAnomaly; }
        }

        public void SetKeplerCoordinatesFromOrbitModule(OrbitModule orbit)
        {
            _keplerCoordinates = KeplerCoordinates.fromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination + 90, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
        }

        public void SetKeplerCoordinatesFromTrueAnomaly(float ecc, float a, float i, float p, float l, float trueAnomaly)
        {
            _keplerCoordinates = KeplerCoordinates.fromTrueAnomaly(ecc, a, i + 90, p, l, trueAnomaly);
        }

        public override string ToString()
        {
            return $"ParameterizedAstroObject: Eccentricity {Eccentricity}, SemiMajorAxis {SemiMajorAxis}, Inclination {Inclination}, ArgumentOfPeriapsis {ArgumentOfPeriapsis}, LongitudeOfAscendingNode {LongitudeOfAscendingNode}, TrueAnomaly {TrueAnomaly}";
        }
    }
}
