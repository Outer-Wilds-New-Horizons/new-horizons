using UnityEngine;
namespace NewHorizons.Utility
{
    public static class CoordinateUtilities
    {
        // Longitude and latitude are in degrees
        // Using the phi and theta convention used on https://mathworld.wolfram.com/SphericalCoordinates.html (Mathematics not physics convention)
        public static Vector3 CartesianToSpherical(Vector3 v, bool useUnityCoords = true)
        {
            float x, y, z;
            if (useUnityCoords)
            {
                // Y is up in unity
                x = v.x;
                y = v.z;
                z = v.y;
            }
            else
            {
                x = v.x;
                y = v.y;
                z = v.z;
            }

            float dist = Mathf.Sqrt(x * x + y * y + z * z);

            // theta
            float longitude = 180f;
            if (x > 0) longitude = Mathf.Rad2Deg * Mathf.Atan(y / x);
            if (x < 0) longitude = Mathf.Rad2Deg * (Mathf.Atan(y / x) + Mathf.PI);

            // phi
            float latitude = (Mathf.Rad2Deg * Mathf.Acos(z / dist));

            return new Vector3(longitude, latitude, dist);
        }

        public static Vector3 SphericalToCartesian(Vector3 v, bool useUnityCoords = true)
        {
            var longitude = v.x;
            var latitude = v.y;
            var r = v.z;

            var theta = Mathf.Deg2Rad * longitude;
            var phi = Mathf.Deg2Rad * latitude;

            float x, y, z;

            if (useUnityCoords)
            {
                x = r * Mathf.Cos(theta) * Mathf.Sin(phi);
                z = r * Mathf.Sin(theta) * Mathf.Sin(phi);
                y = r * Mathf.Cos(phi);
            }
            else
            {
                x = r * Mathf.Cos(theta) * Mathf.Sin(phi);
                y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
                z = r * Mathf.Cos(phi);
            }

            return new Vector3(x, y, z);
        }
    }
}
