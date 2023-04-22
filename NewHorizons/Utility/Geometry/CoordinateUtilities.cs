using UnityEngine;
namespace NewHorizons.Utility.Geometry
{
    public static class CoordinateUtilities
    {
        // Longitude and latitude are in degrees
        // Using the phi and theta convention used on https://mathworld.wolfram.com/SphericalCoordinates.html (Mathematics not physics convention)
        public static Vector3 CartesianToSpherical(Vector3 v, bool useShaderCoords = false)
        {
            float x, y, z;
            if (useShaderCoords)
            {
                // shader does some jank stuff to match standard shader
                x = -v.z;
                y = v.x;
                z = -v.y;
            }
            else
            {
                // Y is up in unity
                x = v.x;
                y = v.z;
                z = v.y;
            }

            float dist = Mathf.Sqrt(x * x + y * y + z * z);

            // theta
            var longitude = Mathf.Rad2Deg * Mathf.Atan2(y, x);

            // phi
            float latitude = Mathf.Rad2Deg * Mathf.Acos(z / dist);

            return new Vector3(longitude, latitude, dist);
        }

        public static Vector3 SphericalToCartesian(Vector3 v)
        {
            var longitude = v.x;
            var latitude = v.y;
            var r = v.z;

            var theta = Mathf.Deg2Rad * longitude;
            var phi = Mathf.Deg2Rad * latitude;

            float x, y, z;

            x = r * Mathf.Cos(theta) * Mathf.Sin(phi);
            z = r * Mathf.Sin(theta) * Mathf.Sin(phi);
            y = r * Mathf.Cos(phi);

            return new Vector3(x, y, z);
        }
    }
}
