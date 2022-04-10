using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Orbital
{
    public class OrbitalParameters
    {
        public float Inclination { get; private set; }
        public float SemiMajorAxis { get; private set; }
        public float LongitudeOfAscendingNode { get; private set; }
        public float Eccentricity { get; private set; }
        public float ArgumentOfPeriapsis { get; private set; }
        public float TrueAnomaly { get; private set; }
        public float MeanAnomaly { get; private set; }
        public float EccentricAnomaly { get; private set; }
        public float Period { get; private set; }

        public Vector3 InitialPosition { get; private set; }
        public Vector3 InitialVelocity { get; private set; }


        public static OrbitalParameters FromTrueAnomaly(Gravity primaryGravity, Gravity secondaryGravity, float eccentricity, float semiMajorAxis, float inclination, float argumentOfPeriapsis, float longitudeOfAscendingNode, float trueAnomaly)
        {
            var orbitalParameters = new OrbitalParameters();
            orbitalParameters.Inclination = inclination;   
            orbitalParameters.SemiMajorAxis = semiMajorAxis;
            orbitalParameters.LongitudeOfAscendingNode = longitudeOfAscendingNode;
            orbitalParameters.Eccentricity = eccentricity;
            orbitalParameters.ArgumentOfPeriapsis = argumentOfPeriapsis;    
            orbitalParameters.TrueAnomaly = trueAnomaly;

            // Have to calculate the rest

            var primaryMass = primaryGravity.Mass;
            var secondaryMass = secondaryGravity.Mass;

            var power = primaryGravity.Power;
            var period = (float) (GravityVolume.GRAVITATIONAL_CONSTANT * (primaryMass + secondaryMass) / (4 * Math.PI * Math.PI * Math.Pow(semiMajorAxis, power)));

            var f = Mathf.Deg2Rad * trueAnomaly; // True anomaly in radians
            var eccentricAnomaly = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Sqrt(1 - eccentricity * eccentricity) * Mathf.Sin(f), 1 + eccentricity * Mathf.Cos(f));

            var meanAnomaly = eccentricAnomaly - eccentricity * Mathf.Sin(Mathf.Deg2Rad * eccentricAnomaly);

            orbitalParameters.Period = period;
            orbitalParameters.MeanAnomaly = meanAnomaly;
            orbitalParameters.EccentricAnomaly = eccentricAnomaly;

            // Position
            var ap = Mathf.Deg2Rad * argumentOfPeriapsis; // Argument of periapsis in radians
            var la = Mathf.Deg2Rad * longitudeOfAscendingNode; // Longitude of ascending node in radians
            var i = Mathf.Deg2Rad * inclination; // Inclination in radians

            var p = semiMajorAxis * (1 - eccentricity * eccentricity); // Semi-latus rectum
            var r = p / (1 + eccentricity * Mathf.Cos(f));
            var mu = GravityVolume.GRAVITATIONAL_CONSTANT * primaryMass;
            var h = Mathf.Sqrt(mu * p);

            var x = r * (Mathf.Cos(la) * Mathf.Cos(ap + f) - Mathf.Sin(la) * Mathf.Sin(ap + f) * Mathf.Cos(i));
            var y = r * (Mathf.Sin(la) * Mathf.Cos(ap + f) - Mathf.Cos(la) * Mathf.Sin(ap + f) * Mathf.Cos(i));
            var z = r * (Mathf.Sin(i) * Mathf.Sin(ap + f));
            orbitalParameters.InitialPosition = new Vector3(x, z, y); 

            var coefficient = h * eccentricity * Mathf.Sin(f) / (r * p);
            var vx = x * coefficient - (h / r) * (Mathf.Cos(la) * Mathf.Sin(ap + f) + Mathf.Sin(la) * Mathf.Cos(ap + f) * Mathf.Cos(i));
            var vy = y * coefficient - (h / r) * (Mathf.Sin(la) * Mathf.Sin(ap + f) - Mathf.Cos(la) * Mathf.Cos(ap + f) * Mathf.Cos(i));
            var vz = z * coefficient + (h / r) * (Mathf.Sin(i) * Mathf.Cos(ap + f));
            orbitalParameters.InitialVelocity = new Vector3(vx, vz, vy);

            return orbitalParameters;
        }
    }
}
