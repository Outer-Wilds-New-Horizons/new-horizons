using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.External;
using CRGravity = PacificEngine.OW_CommonResources.Geometry.Orbits.Gravity;
using KeplerCoordinates = PacificEngine.OW_CommonResources.Geometry.Orbits.KeplerCoordinates;

/*
 * Wrapper class for OW_CommonResources.Geometry.Orbits functions
 */
namespace NewHorizons.OrbitalPhysics
{
    public static class OrbitalHelper
    {
        public enum FalloffType
        {
            inverseSquared,
            linear,
            none
        }

        public static Vector3 GetPosition(OrbitModule orbit)
        {
            Vector3 pos = Vector3.zero;
            if(orbit.SemiMajorAxis != 0)
            {
                // The gravity doesnt have an affect on position so we put whatever
                var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, 1f, 100f);
                var kepler = KeplerCoordinates.fromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
                pos = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler).Item1;
            }

            return pos;
        }

        public static Vector3 GetPosition(ParameterizedAstroObject ao)
        {
            Vector3 pos = Vector3.zero;
            if(ao.SemiMajorAxis != 0)
            {
                var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, 1f, 100f);
                var kepler = KeplerCoordinates.fromTrueAnomaly(ao.Eccentricity, ao.SemiMajorAxis, ao.Inclination, ao.ArgumentOfPeriapsis, ao.LongitudeOfAscendingNode, ao.TrueAnomaly);
                pos = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler).Item1;
            }

            return pos;
        }

        public static Vector3 GetPositionFromTrueAnomaly(float eccentricity, float semiMajorAxis, float inclination, float argumentOfPeriapsis, float longitudeOfAscendingNode, float trueAnomaly)
        {
            Vector3 pos = Vector3.zero;
            if (semiMajorAxis != 0)
            {
                var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, 1f, 100f);
                var kepler = KeplerCoordinates.fromTrueAnomaly(eccentricity, semiMajorAxis, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, trueAnomaly);
                pos = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler).Item1;
            }

            return pos;
        }

        public static Vector3 GetPositionFromEccentricAnomaly(float eccentricity, float semiMajorAxis, float inclination, float argumentOfPeriapsis, float longitudeOfAscendingNode, float eccentricAnomaly)
        {
            Vector3 pos = Vector3.zero;
            if (semiMajorAxis != 0)
            {
                var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, 1f, 100f);
                var kepler = KeplerCoordinates.fromEccentricAnomaly(eccentricity, semiMajorAxis, inclination, argumentOfPeriapsis, longitudeOfAscendingNode, eccentricAnomaly);
                pos = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler).Item1;
            }

            return pos;
        }

        public static Vector3 GetVelocity(Gravity gravity, OrbitModule orbit)
        {
            var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, gravity.Exponent, gravity.Mass);
            var kepler = KeplerCoordinates.fromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
            var vel = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler).Item2;
            return vel;
        }

        public static Vector3 GetVelocity(Gravity gravity, ParameterizedAstroObject ao)
        {
            var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, gravity.Exponent, gravity.Mass);
            var kepler = KeplerCoordinates.fromTrueAnomaly(ao.Eccentricity, ao.SemiMajorAxis, ao.Inclination, ao.ArgumentOfPeriapsis, ao.LongitudeOfAscendingNode, ao.TrueAnomaly);
            var vel = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler).Item2;
            return vel;
        }

        public static KeplerCoordinates KeplerCoordinatesFromOrbitModule(OrbitModule orbit)
        {
            return KeplerCoordinates.fromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
        }

        public class Gravity
        {
            public float Exponent { get; }
            public float Mass { get; }

            public Gravity(float exponent, float mass)
            {
                Exponent = exponent;
                Mass = mass;
            }

            public Gravity(GravityVolume gv)
            {
                Exponent = gv.GetFalloffExponent();
                Mass = gv.GetStandardGravitationalParameter() / GravityVolume.GRAVITATIONAL_CONSTANT;
            }
        }
    }
}
