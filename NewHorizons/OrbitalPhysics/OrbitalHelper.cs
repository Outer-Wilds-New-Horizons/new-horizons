using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.External;

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

        public static Vector3 RotateTo(Vector3 vector, KeplerElements elements)
        {
            // For now, eccentric orbits gotta start at apoapsis and cant be inclined
            var rot = Quaternion.AngleAxis(elements.LongitudeOfAscendingNode + elements.TrueAnomaly + elements.ArgumentOfPeriapsis + 180f, Vector3.up);
            if (elements.Eccentricity != 0)
            {
                rot = Quaternion.AngleAxis(elements.LongitudeOfAscendingNode + elements.ArgumentOfPeriapsis + 180f, Vector3.up);
            }

            var incAxis = Quaternion.AngleAxis(elements.LongitudeOfAscendingNode, Vector3.up) * Vector3.left;
            var incRot = Quaternion.AngleAxis(elements.Inclination, incAxis);

            return rot * incRot * vector;
        }

        public static Vector3 RotateTo(Vector3 vector, OrbitModule module)
        {
            return RotateTo(vector, KeplerElements.FromOrbitModule(module));
        }

        public static Vector3 VelocityDirection(Vector3 separation, KeplerElements elements)
        {
            var incAxis = Quaternion.AngleAxis(elements.LongitudeOfAscendingNode, Vector3.up) * Vector3.left;
            var incRot = Quaternion.AngleAxis(elements.Inclination, incAxis);
            return Vector3.Cross(RotateTo(Vector3.up, elements), separation);
        }

        public static float GetOrbitalVelocity(float distance, Gravity gravity, KeplerElements kepler)
        {
            if (kepler.Eccentricity == 0) return GetCircularOrbitVelocity(distance, gravity, kepler);

            if (gravity.Exponent == 2)
            {
                return Mathf.Sqrt(GravityVolume.GRAVITATIONAL_CONSTANT * gravity.Mass * (2f / distance - 1f / kepler.SemiMajorAxis));
            }
            if(gravity.Exponent == 1)
            {
                var mu = GravityVolume.GRAVITATIONAL_CONSTANT * gravity.Mass;
                var rp2 = kepler.Periapsis * kepler.Periapsis;
                var ra2 = kepler.Apoapsis * kepler.Apoapsis;
                float term1 = 0;
                if(kepler.Eccentricity < 1)
                    term1 = mu * Mathf.Log(kepler.Periapsis / kepler.Apoapsis) * rp2 / (rp2 - ra2);
                var term2 = mu * Mathf.Log(kepler.Apoapsis / distance);
                return Mathf.Sqrt(2 * (term1 + term2));
            }
            Logger.LogError($"Invalid exponent {gravity.Exponent}");
            return 0f;
        }

        public static float GetCircularOrbitVelocity(float distance, Gravity gravity, KeplerElements kepler)
        {
            if (gravity.Exponent == 2)
            {
                return Mathf.Sqrt(GravityVolume.GRAVITATIONAL_CONSTANT * gravity.Mass / distance);
            }
            if(gravity.Exponent == 1)
            {
                return Mathf.Sqrt(GravityVolume.GRAVITATIONAL_CONSTANT * gravity.Mass);
            }
            Logger.LogError($"Invalid exponent {gravity.Exponent}");
            return 0f;
        }
    }
}
