using UnityEngine;
namespace NewHorizons.Utility
{
    public static class CoordinateUtilities
    {
        // Longitude and latitude are in degrees
        // Using the phi and theta convention used on https://mathworld.wolfram.com/SphericalCoordinates.html (Mathematics not physics convention)
        public static Vector3 CartesianToSpherical(Vector3 v)
        {
            // Y is up in unity

            float dist = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);

            // theta
            float longitude = 180f;
            if (v.x > 0) longitude = Mathf.Rad2Deg * Mathf.Atan(v.z / v.x);
            if (v.x < 0) longitude = Mathf.Rad2Deg * (Mathf.Atan(v.z / v.x) + Mathf.PI);

            // phi
            float latitude = (Mathf.Rad2Deg * Mathf.Acos(v.y / dist));

            return new Vector3(longitude, latitude, dist);
        }

        public static Vector3 SphericalToCartesian(Vector3 v)
        {
            var longitude = v.x;
            var latitude = v.y;
            var r = v.z;

            var theta = Mathf.Deg2Rad * longitude;
            var phi = Mathf.Deg2Rad * latitude;

            var x = r * Mathf.Cos(theta) * Mathf.Sin(phi);
            var z = r * Mathf.Sin(theta) * Mathf.Sin(phi);
            var y = r * Mathf.Cos(phi);

            return new Vector3(x, y, z);
        }
    }
}
