using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.OrbitalPhysics
{
    public class KeplerElements
    {
        public float LongitudeOfAscendingNode { get; }
        public float Eccentricity { get; }
        public float SemiMajorAxis { get; }
        public float Inclination { get; }
        public float ArgumentOfPeriapsis { get; }
        public float TrueAnomaly { get; }
        public float EccentricAnomaly { get; }
        public float MeanAnomaly { get; }
        public float SemiMinorAxis { get; }
        public float Focus { get; }
        public float Apoapsis { get; }
        public float Periapsis { get; }

        private KeplerElements(float e, float a, float i, float longitudeOfAscendingNode, float argumentOfPeriapsis, float trueAnomaly, float eccentricAnomaly, float meanAnomaly)
        {
            LongitudeOfAscendingNode = longitudeOfAscendingNode;
            Eccentricity = e;
            SemiMajorAxis = a;
            Inclination = i;
            ArgumentOfPeriapsis = argumentOfPeriapsis;
            TrueAnomaly = trueAnomaly;

            SemiMinorAxis = SemiMajorAxis * Mathf.Sqrt(1 - Eccentricity * Eccentricity);
            Focus = Mathf.Sqrt((SemiMajorAxis * SemiMajorAxis) - (SemiMinorAxis * SemiMinorAxis));
            Apoapsis = SemiMajorAxis + Focus;
            Periapsis = SemiMajorAxis - Focus;

            EccentricAnomaly = eccentricAnomaly;
            MeanAnomaly = meanAnomaly;
        }

        public static KeplerElements FromOrbitModule(OrbitModule module)
        {
            return FromTrueAnomaly(module.Eccentricity, module.SemiMajorAxis, module.Inclination, module.LongitudeOfAscendingNode, module.ArgumentOfPeriapsis, module.TrueAnomaly);
        }

        public static KeplerElements FromTrueAnomaly(float e, float a, float i, float longitudeOfAscendingNode, float argumentOfPeriapsis, float trueAnomaly)
        {
            var eccentricAnomaly = EccentricAnomalyFromTrueAnomaly(trueAnomaly, e);
            var meanAnomaly = MeanAnomalyFromEccentricAnomaly(eccentricAnomaly, e);
            return new KeplerElements(e, a, i, longitudeOfAscendingNode, argumentOfPeriapsis, trueAnomaly, eccentricAnomaly, meanAnomaly);    
        }

        public static KeplerElements FromMeanAnomaly(float e, float a, float i, float longitudeOfAscendingNode, float argumentOfPeriapsis, float meanAnomaly)
        {
            var trueAnomaly = TrueAnomalyFromMeanAnomaly(meanAnomaly, e);
            var eccentricAnomaly = EccentricAnomalyFromTrueAnomaly(trueAnomaly, e);
            return new KeplerElements(e, a, i, longitudeOfAscendingNode, argumentOfPeriapsis, trueAnomaly, eccentricAnomaly, meanAnomaly);
        }

        public static KeplerElements FromEccentricAnomaly(float e, float a, float i, float longitudeOfAscendingNode, float argumentOfPeriapsis, float eccentricAnomaly)
        {
            var trueAnomaly = TrueAnomalyFromEccentricAnomaly(eccentricAnomaly, e);
            var meanAnomaly = MeanAnomalyFromEccentricAnomaly(eccentricAnomaly, e);
            return new KeplerElements(e, a, i, longitudeOfAscendingNode, argumentOfPeriapsis, trueAnomaly, eccentricAnomaly, meanAnomaly);
        }

        private static float MeanAnomalyFromEccentricAnomaly(float eccentricAnomaly, float eccentricity)
        {
            return eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly);
        }

        private static float TrueAnomalyFromEccentricAnomaly(float eccentricAnomaly, float eccentricity)
        {
            var a = Mathf.Cos(eccentricAnomaly) - eccentricity;
            var h = 1 - eccentricity * Mathf.Cos(eccentricAnomaly);
            var o = h * h - a * a;
            return Mathf.Atan2(o, a);
        }

        private static float EccentricAnomalyFromTrueAnomaly(float trueAnomaly, float eccentricity)
        {
            var a = Mathf.Cos(trueAnomaly) + eccentricity;
            var h = 1 + eccentricity * Mathf.Cos(trueAnomaly);
            var o = h * h - a * a;
            return Mathf.Atan2(o, a);
        }

        private static float TrueAnomalyFromMeanAnomaly(float meanAnomaly, float eccentricity)
        {
            // Fourier expansion
            var term1 = meanAnomaly;
            var term2 = (2 * eccentricity - eccentricity * eccentricity * eccentricity / 4f) * Mathf.Sin(meanAnomaly);
            var term3 = (eccentricity * eccentricity * Mathf.Sin(2 * meanAnomaly) * 5f / 4f);
            var term4 = (eccentricity * eccentricity * eccentricity * Mathf.Sin(3 * meanAnomaly) * 13f / 12f);
            return term1 + term2 + term3 + term4;
        }
    }
}
