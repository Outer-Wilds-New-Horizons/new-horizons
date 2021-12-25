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
        public float TrueAnomaly { get; private set; }
        public float EccentricAnomaly { get; private set; }
        public float MeanAnomaly { get; private set; }
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
            var newKeplerElements = new KeplerElements(e, a, i, longitudeOfAscendingNode, argumentOfPeriapsis, 0, 0, 0);
            newKeplerElements.SetTrueAnomaly(trueAnomaly);
            return newKeplerElements;
        }

        public static KeplerElements FromMeanAnomaly(float e, float a, float i, float longitudeOfAscendingNode, float argumentOfPeriapsis, float meanAnomaly)
        {
            var newKeplerElements = new KeplerElements(e, a, i, longitudeOfAscendingNode, argumentOfPeriapsis, 0, 0, 0);
            newKeplerElements.SetMeanAnomaly(meanAnomaly);
            return newKeplerElements;
        }

        public static KeplerElements FromEccentricAnomaly(float e, float a, float i, float longitudeOfAscendingNode, float argumentOfPeriapsis, float eccentricAnomaly)
        {
            var newKeplerElements = new KeplerElements(e, a, i, longitudeOfAscendingNode, argumentOfPeriapsis, 0, 0, 0);
            newKeplerElements.SetEccentricAnomaly(eccentricAnomaly);
            return newKeplerElements;
        }

        public static KeplerElements Copy(KeplerElements original)
        {
            return KeplerElements.FromTrueAnomaly(original.Eccentricity, original.SemiMajorAxis, original.Inclination, original.LongitudeOfAscendingNode, original.ArgumentOfPeriapsis, original.TrueAnomaly);
        }

        public void SetTrueAnomaly(float trueAnomaly)
        {
            TrueAnomaly = trueAnomaly;
            EccentricAnomaly = EccentricAnomalyFromTrueAnomaly(trueAnomaly, Eccentricity);
            MeanAnomaly = MeanAnomalyFromEccentricAnomaly(EccentricAnomaly, Eccentricity);
        }

        public void SetMeanAnomaly(float meanAnomaly)
        {
            MeanAnomaly = meanAnomaly;
            TrueAnomaly = TrueAnomalyFromMeanAnomaly(meanAnomaly, Eccentricity);
            EccentricAnomaly = EccentricAnomalyFromTrueAnomaly(TrueAnomaly, Eccentricity);
        }

        public void SetEccentricAnomaly(float eccentricAnomaly)
        {
            EccentricAnomaly = eccentricAnomaly;
            TrueAnomaly = TrueAnomalyFromEccentricAnomaly(eccentricAnomaly, Eccentricity);
            MeanAnomaly = MeanAnomalyFromEccentricAnomaly(eccentricAnomaly, Eccentricity);
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
