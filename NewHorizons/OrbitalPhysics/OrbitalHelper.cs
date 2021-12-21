using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.OrbitalPhysics
{
    public static class OrbitalHelper
    {
        public enum FalloffType
        {
            inverseSquared,
            linear
        }

        public static CartesianStateVectors CartesianStateVectorsFromTrueAnomaly(float standardGraviationalParameter, float eccentricity, float semiMajorAxis, float inclination,
            float longitudeOfAscendingNode, float argumentOfPeriapsis, float trueAnomaly, FalloffType falloffType)
        {
            var nu = Mathf.Deg2Rad * trueAnomaly;
            var E = Mathf.Atan2(Mathf.Sqrt(1 - eccentricity * eccentricity) * Mathf.Sin(nu), (eccentricity + Mathf.Cos(nu)));

            return CartesianStateVectorsFromOrbitalElements(standardGraviationalParameter, eccentricity, semiMajorAxis, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, E, nu, falloffType);
        }

        public static CartesianStateVectors CartesianStateVectorsFromEccentricAnomaly(float standardGraviationalParameter, float eccentricity, float semiMajorAxis, float inclination,
            float longitudeOfAscendingNode, float argumentOfPeriapsis, float eccentricAnomaly, FalloffType falloffType)
        {
            var E = Mathf.Deg2Rad * eccentricAnomaly;
            var nu = 2f * Mathf.Atan2(Mathf.Sqrt(1 + eccentricity) * Mathf.Sin(E)/2f, Mathf.Sqrt(1 - eccentricity) * Mathf.Cos(E) / 2f);

            return CartesianStateVectorsFromOrbitalElements(standardGraviationalParameter, eccentricity, semiMajorAxis, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, E, nu, falloffType);
        }

        private static CartesianStateVectors CartesianStateVectorsFromOrbitalElements(float standardGraviationalParameter, float eccentricity, float semiMajorAxis, float inclination,
            float longitudeOfAscendingNode, float argumentOfPeriapsis, float E, float nu, FalloffType falloffType)
        {
            //Keplerian Orbit Elements −→ Cartesian State Vectors (Memorandum #1) Rene Schwarz

            if (semiMajorAxis == 0) return new CartesianStateVectors();

            // POS
            var r = semiMajorAxis * (1 - eccentricity * Mathf.Cos(E));

            Vector3 position = r * new Vector3(Mathf.Cos(nu), 0, Mathf.Sin(nu));

            // VEL
            var speed = Visviva(falloffType, standardGraviationalParameter, position.magnitude, semiMajorAxis, eccentricity);
            Vector3 velocity = speed * (new Vector3(-Mathf.Sin(E), 0, Mathf.Sqrt(1 - (eccentricity * eccentricity)) * Mathf.Cos(E))).normalized;

            Quaternion periapsisRot = Quaternion.AngleAxis(Mathf.Deg2Rad * argumentOfPeriapsis, Vector3.up);
            var inclinationAxis = Quaternion.AngleAxis(Mathf.Repeat(Mathf.Deg2Rad * longitudeOfAscendingNode, 2f * Mathf.PI), Vector3.up) * Vector3.left;
            Quaternion inclinationRot = Quaternion.AngleAxis(inclination, inclinationAxis);

            position = periapsisRot * inclinationRot * position;
            velocity = periapsisRot * inclinationRot * velocity;

            return new CartesianStateVectors(position, velocity);
        }

        public static float Visviva(FalloffType falloffType, float standardGravitationalParameter, float dist, float semiMajorAxis, float eccentricity)
        {
            switch(falloffType)
            {
                case FalloffType.inverseSquared:
                    return Mathf.Sqrt(standardGravitationalParameter * (2f / dist - 1f / semiMajorAxis));
                case FalloffType.linear:
                    if (eccentricity == 0f) return Mathf.Sqrt(standardGravitationalParameter);

                    var ra = semiMajorAxis * (1 + eccentricity);
                    var rp = semiMajorAxis * (1 - eccentricity);

                    var kineticEneregyAtApoapsis = standardGravitationalParameter * Mathf.Log(ra / rp) * (rp * rp) / ((rp * rp) - (ra * ra));
                    var gravitationalEnergy = standardGravitationalParameter * Mathf.Log(dist / ra);
                    var v = Mathf.Sqrt(2 * (kineticEneregyAtApoapsis + gravitationalEnergy));

                    return v;
            }
            Logger.LogError($"Invalid falloffType {falloffType}");
            return 0f;
        }

        private static Vector3 RotateToOrbitalPlane(Vector3 vector, float longitudeOfAscendingNode, float argumentOfPeriapsis, float inclination)
        {
            vector = Quaternion.AngleAxis(Mathf.Deg2Rad * argumentOfPeriapsis, Vector3.up) * vector;

            var inclinationAxis = Quaternion.AngleAxis(Mathf.Repeat(Mathf.Deg2Rad * longitudeOfAscendingNode, 2f * Mathf.PI), Vector3.up) * new Vector3(1, 0, 0);
            vector = Quaternion.AngleAxis(Mathf.Repeat(Mathf.Deg2Rad * inclination, 2f * Mathf.PI), inclinationAxis) * vector;

            return vector;
        }
    }
}
