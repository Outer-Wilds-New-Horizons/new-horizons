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

        public static Tuple<Vector3, Vector3> GetCartesian(Gravity gravity, OrbitModule orbit)
        {
            if (orbit.SemiMajorAxis == 0)
            {
                return Tuple.Create(Vector3.zero, Vector3.zero);
            }

            var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, gravity.Exponent, gravity.Mass);
            var kepler = KeplerCoordinates.fromTrueAnomaly(orbit.Eccentricity, orbit.SemiMajorAxis, orbit.Inclination, orbit.ArgumentOfPeriapsis, orbit.LongitudeOfAscendingNode, orbit.TrueAnomaly);
            var cartesian = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler);

            if (cartesian == null)
            {
                cartesian = Tuple.Create(Vector3.zero, Vector3.zero);
            }

            return cartesian;
        }

        public static Tuple<Vector3, Vector3> GetCartesian(Gravity gravity, ParameterizedAstroObject ao)
        {
            if (ao.SemiMajorAxis == 0)
            {
                return Tuple.Create(Vector3.zero, Vector3.zero);
            }

            var crGravity = new CRGravity(GravityVolume.GRAVITATIONAL_CONSTANT, gravity.Exponent, gravity.Mass);
            var kepler = KeplerCoordinates.fromTrueAnomaly(ao.Eccentricity, ao.SemiMajorAxis, ao.Inclination, ao.ArgumentOfPeriapsis, ao.LongitudeOfAscendingNode, ao.TrueAnomaly);
            var cartesian = PacificEngine.OW_CommonResources.Geometry.Orbits.OrbitHelper.toCartesian(crGravity, 0f, kepler);

            if (cartesian == null)
            {
                cartesian = Tuple.Create(Vector3.zero, Vector3.zero);
            }

            return cartesian;
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
