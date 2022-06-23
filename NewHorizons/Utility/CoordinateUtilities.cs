using UnityEngine;
namespace NewHorizons.Utility
{
    public static class CoordinateUtilities
    {
        // Longitude and latitude are in degrees
        // Using the phi and theta convention used on https://mathworld.wolfram.com/SphericalCoordinates.html (Mathematics not physics convention)
        public static Vector3 CartesianToSpherical(Vector3 v)
        {
            float dist = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);

            // theta
            float longitude = 180f;
            if (v.x > 0) longitude = Mathf.Rad2Deg * Mathf.Atan(v.y / v.x);
            if (v.x < 0) longitude = Mathf.Rad2Deg * (Mathf.Atan(v.y / v.x) + Mathf.PI);

            // phi
            float latitude = (Mathf.Rad2Deg * Mathf.Acos(v.z / dist));

            return new Vector3(longitude, latitude, dist);
        }

        public static Vector3 CartesianToSpherical(float x, float y, float z)
        {
            return CartesianToSpherical(new Vector3(x, y, z));
        }

        public static Vector3 SphericalToCartesian(Vector3 v)
        {
            var longitude = v.x;
            var latitude = v.y;
            var r = v.z;

            return SphericalToCartesian(longitude, latitude, r);
        }

        public static Vector3 SphericalToCartesian(float longitude, float latitude, float radius)
        {
            var theta = Mathf.Deg2Rad * longitude;
            var phi = Mathf.Deg2Rad * latitude;

            var x = radius * Mathf.Cos(theta) * Mathf.Sin(phi);
            var y = radius * Mathf.Sin(theta) * Mathf.Sin(phi);
            var z = radius * Mathf.Cos(phi);

            return new Vector3(x, y, z);
        }
    }
}
