using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Kepler
{
    public static class OrbitalHelper
    {
        public static Vector3 CartesianFromOrbitalElements(float e, float a, float inclination, float ascending, float periapsis, float angle)
        {
            float b = a * Mathf.Sqrt(1f - (e * e));

            // Get point on ellipse
            // Angle starts at apoapsis, shift by 90 degrees to get to periapsis and add true anomaly
            float ellipseAngle = Mathf.Repeat(Mathf.PI / 4f + Mathf.Deg2Rad * angle, 2 * Mathf.PI);

            float tAngle = Mathf.Tan(ellipseAngle);
            float x = (a * b) / Mathf.Sqrt(b * b + a * a * tAngle * tAngle);
            if (ellipseAngle > Mathf.PI / 2f && ellipseAngle < 3f * Mathf.PI / 2f) x *= -1;
            float y = x * tAngle;

            // Fix limits
            if (float.IsNaN(x)) {
                x = 0;
                if (angle < 180) y = b;
                else y = -b;
            }

            var position = new Vector3(x, 0, y);

            return RotateToOrbitalPlane(position, ascending, periapsis, inclination);
        }

        public static float VisViva(float standardGravitationalParameter, Vector3 relativePosition, float semiMajorAxis)
        {
            return Mathf.Sqrt(standardGravitationalParameter * (2f / relativePosition.magnitude - 1f / semiMajorAxis));
        }

        public static Vector3 EllipseTangent(float e, float a, float inclination, float ascending, float periapsis, float angle)
        {
            float b = a * Mathf.Sqrt(1f - (e * e));
            float ellipseAngle = Mathf.Repeat(Mathf.PI / 4f + Mathf.Deg2Rad * angle, 2 * Mathf.PI);

            var tan = Mathf.Tan(ellipseAngle);

            float x = (a * b) / Mathf.Sqrt(b * b + a * a * tan * tan);

            var sec2 = 1f / (tan * tan);
            float dxdt = -a * a * a * b * sec2 * tan / Mathf.Pow(a * a * tan * tan + b * b, 3f / 2f);

            if (ellipseAngle > Mathf.PI / 2f && ellipseAngle < 3f * Mathf.PI / 2f)
            {
                dxdt *= -1;
                x *= -1;
            }

            // Product rule
            var dydt = sec2 * x + dxdt * tan;

            // Fix limits
            if(float.IsNaN(dxdt))
            {
                dydt = 0;
                if (angle == Mathf.PI / 2f) dxdt = -1;
                else dxdt = 1;
            }

            var vector = new Vector3(dxdt, 0, dydt).normalized;
            return RotateToOrbitalPlane(vector, ascending, periapsis, inclination);
        }

        private static Vector3 RotateToOrbitalPlane(Vector3 vector, float ascending, float periapsis, float inclination)
        {
            // Periapsis is at 90 degrees
            vector = Quaternion.AngleAxis(Mathf.Deg2Rad * (ascending + periapsis) + Mathf.PI / 2f, Vector3.up) * vector;

            var inclinationAxis = Quaternion.AngleAxis(Mathf.Repeat(Mathf.Deg2Rad * ascending, 2f * Mathf.PI), Vector3.up) * new Vector3(1, 0, 0);
            vector = Quaternion.AngleAxis(Mathf.Repeat(Mathf.Deg2Rad * inclination, 2f * Mathf.PI), inclinationAxis) * vector;

            return vector;
        }

        public static Vector3 CalculateOrbitVelocity(OWRigidbody primaryBody, Vector3 relativePosition, float inclination = 0f)
        {
            GravityVolume attachedGravityVolume = primaryBody.GetAttachedGravityVolume();
            if (attachedGravityVolume == null)
            {
                return Vector3.zero;
            }
            Vector3 vector2 = Vector3.Cross(relativePosition, Vector3.up).normalized;
            vector2 = Quaternion.AngleAxis(inclination, relativePosition) * vector2;
            float d = Mathf.Sqrt(attachedGravityVolume.CalculateForceAccelerationAtPoint(relativePosition + primaryBody.transform.position).magnitude * relativePosition.magnitude);
            return vector2 * d;
        }
    }
}
