using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

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

            // All in radians
            var f = Mathf.Deg2Rad * trueAnomaly;
            var eccentricAnomaly = Mathf.Atan2(Mathf.Sqrt(1 - (eccentricity * eccentricity)) * Mathf.Sin(f), eccentricity + Mathf.Cos(f));
            var meanAnomaly = eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly);

            orbitalParameters.Period = period;
            orbitalParameters.MeanAnomaly = meanAnomaly * Mathf.Rad2Deg;
            orbitalParameters.EccentricAnomaly = eccentricAnomaly * Mathf.Rad2Deg;

            var p = semiMajorAxis * (1 - (eccentricity * eccentricity)); // Semi-latus rectum
            var r = p / (1 + eccentricity * Mathf.Cos(f));
            var mu = GravityVolume.GRAVITATIONAL_CONSTANT * primaryMass;
            var h = Mathf.Sqrt(mu * p);
            var v = Mathf.Sqrt(mu * ((2 / r) - (1 / semiMajorAxis)));
            var semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - (eccentricity * eccentricity));
            var focusDistance = Mathf.Sqrt((semiMajorAxis * semiMajorAxis) - (semiMinorAxis * semiMinorAxis));

            var x = semiMajorAxis * Mathf.Cos(eccentricAnomaly) - focusDistance;
            var y = semiMinorAxis * Mathf.Sin(eccentricAnomaly);

            var R1 = Quaternion.AngleAxis(longitudeOfAscendingNode, Vector3.up);
            var R2 = Quaternion.AngleAxis(inclination, Vector3.forward);
            var R3 = Quaternion.AngleAxis(argumentOfPeriapsis, Vector3.up);

            var n_p = new Vector2(x, y).normalized;
            var n_v = new Vector2(-y, x).normalized;

            // Idk why y and z get swapped just go with it
            var pos = new Vector3(r * n_p.x, r * n_p.y, 0f);
            var vel = new Vector3(v * n_v.x, 0f, -v * n_v.y);

            orbitalParameters.InitialPosition = R1 * R2 * R3 * pos;
            orbitalParameters.InitialVelocity = R1 * R2 * R3 * vel;

            Logger.Log($"POSITION: {orbitalParameters.InitialPosition}, {orbitalParameters.InitialVelocity}, {n_p}, {n_v}");

            /*
            var x = r * (Mathf.Cos(la) * Mathf.Cos(ap + f) - Mathf.Sin(la) * Mathf.Sin(ap + f) * Mathf.Cos(i));
            var y = r * (Mathf.Sin(la) * Mathf.Cos(ap + f) + Mathf.Cos(la) * Mathf.Sin(ap + f) * Mathf.Cos(i));
            var z = r * (Mathf.Sin(i) * Mathf.Sin(ap + f));
            orbitalParameters.InitialPosition = new Vector3(x, y, z); 

            // Velocity

            var coefficient = h * eccentricity * Mathf.Sin(f) / (r * p);
            var vx = x * coefficient - (h / r) * (Mathf.Cos(la) * Mathf.Sin(ap + f) + Mathf.Sin(la) * Mathf.Cos(ap + f) * Mathf.Cos(i));
            var vy = y * coefficient - (h / r) * (Mathf.Sin(la) * Mathf.Sin(ap + f) - Mathf.Cos(la) * Mathf.Cos(ap + f) * Mathf.Cos(i));
            var vz = z * coefficient + (h / r) * (Mathf.Sin(i) * Mathf.Cos(ap + f));
            orbitalParameters.InitialVelocity = new Vector3(vx, vy, vz);
            */

            /*
            // Where x points towards the periapsis 
            var ox = semiMajorAxis * Mathf.Cos(f);
            var oy = semiMinorAxis * Mathf.Sin(f);
            var o = new Vector3(ox, 0, oy);

            var mu = GravityVolume.GRAVITATIONAL_CONSTANT * primaryMass;
            var r = o.magnitude;
            var o_dot_coeff = Mathf.Sqrt(mu * ((2/r) - (1/semiMajorAxis)));
            var ox_dot = o_dot_coeff * -Mathf.Sin(eccentricAnomaly);
            var oy_dot = o_dot_coeff * Mathf.Cos(eccentricAnomaly);
            var o_dot = new Vector3(ox_dot, 0, oy_dot);

            // Do some rotations
            var R1 = Quaternion.AngleAxis(Mathf.Rad2Deg * -longitudeOfAscendingNode, Vector3.up);
            var R2 = Quaternion.AngleAxis(Mathf.Rad2Deg * -inclination, Vector3.forward);
            var R3 = Quaternion.AngleAxis(Mathf.Rad2Deg * -argumentOfPeriapsis, Vector3.up);

            orbitalParameters.InitialPosition = R1 * R2 * R3 * o;
            orbitalParameters.InitialVelocity = R1 * R2 * R3 * o_dot;
            */

            return orbitalParameters;
        }
    }
}
