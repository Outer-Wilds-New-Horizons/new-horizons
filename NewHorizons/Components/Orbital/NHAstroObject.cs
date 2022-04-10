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

        public void SetOrbitalParametersFromConfig(OrbitModule orbit)
        {
            SetOrbitalParametersFromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
        }

        public void SetOrbitalParametersFromTrueAnomaly(float ecc, float a, float i, float p, float l, float trueAnomaly)
        {
            Inclination = ecc;
            SemiMajorAxis = a;
            LongitudeOfAscendingNode = l;
            Inclination = i;
            Eccentricity = ecc;
            ArgumentOfPeriapsis = p;
            TrueAnomaly = trueAnomaly;
        }

        public override string ToString()
        {
            return $"ParameterizedAstroObject: Eccentricity {Eccentricity}, SemiMajorAxis {SemiMajorAxis}, Inclination {Inclination}, ArgumentOfPeriapsis {ArgumentOfPeriapsis}, LongitudeOfAscendingNode {LongitudeOfAscendingNode}, TrueAnomaly {TrueAnomaly}";
        }

        public OrbitalParameters GetOrbitalParameters(Gravity primaryGravity, Gravity secondaryGravity)
        {
            return OrbitalParameters.FromTrueAnomaly(primaryGravity, secondaryGravity, Eccentricity, SemiMajorAxis, Inclination, ArgumentOfPeriapsis, LongitudeOfAscendingNode, TrueAnomaly);
        }
    }
}
