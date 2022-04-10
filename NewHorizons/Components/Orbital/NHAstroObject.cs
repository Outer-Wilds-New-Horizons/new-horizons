using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Components.Orbital
{
    public class NHAstroObject : AstroObject, IOrbitalParameters
    {
        public float Inclination { get; set; }
        public float SemiMajorAxis { get; set; }
        public float LongitudeOfAscendingNode { get; set; }
        public float Eccentricity { get; set; }
        public float ArgumentOfPeriapsis { get; set; }
        public float TrueAnomaly { get; set; }
        public bool HideDisplayName { get; set; }

        public void SetOrbitalParametersFromOrbitModule(OrbitModule orbit)
        {
            SetOrbitalParametersFromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
        }

        public void SetOrbitalParametersFromTrueAnomaly(float ecc, float a, float i, float p, float l, float trueAnomaly)
        {
            var orbitalParameters = OrbitalParameters.FromTrueAnomaly(ecc, a, i, p, l, trueAnomaly);
            Inclination = orbitalParameters.Inclination;
            SemiMajorAxis = orbitalParameters.SemiMajorAxis;
            LongitudeOfAscendingNode = orbitalParameters.LongitudeOfAscendingNode;
            Eccentricity = orbitalParameters.Eccentricity;
            ArgumentOfPeriapsis = orbitalParameters.ArgumentOfPeriapsis;
            TrueAnomaly = orbitalParameters.TrueAnomaly;
        }

        public override string ToString()
        {
            return $"ParameterizedAstroObject: Eccentricity {Eccentricity}, SemiMajorAxis {SemiMajorAxis}, Inclination {Inclination}, ArgumentOfPeriapsis {ArgumentOfPeriapsis}, LongitudeOfAscendingNode {LongitudeOfAscendingNode}, TrueAnomaly {TrueAnomaly}";
        }

        public OrbitalParameters GetOrbitalParameters()
        {
            return OrbitalParameters.FromTrueAnomaly(Eccentricity, SemiMajorAxis, Inclination, ArgumentOfPeriapsis, LongitudeOfAscendingNode, TrueAnomaly);
        }
    }
}
